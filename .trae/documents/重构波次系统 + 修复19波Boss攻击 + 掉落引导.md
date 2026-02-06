## 现状梳理与调用链输出（会产出文档）
- 梳理并输出“当前波次配置表 → 运行时对象”的对照表：
  - 配置源：`Assets/StreamingAssets/Config/EnemyWaveConfig.csv`
  - 解析：`EnemyWaveConfigLoader.ParseCSV()` → `EnemyWaveConfigRow`
  - 转换：`EnemyWave.FromConfigRow()`（定义在 [LevelConfig.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/Config/LevelConfig.cs)）
  - 运行：`EnemyGenerator.Start()` → `LoadFromCSVAsync()` → `EnemyGenerator.Update()` 生成/结束
- 绘制完整调用链与关键数据字段语义（包含：怪物种类、数量/时长、刷新间隔、混合标记、切波条件），用 Mermaid 画出流程图/状态图，并落盘为项目文档（例如 `Docs/WaveSystem.md`）。

## （1）波次系统彻底重构（满足 A/B 策略 + 状态机 + 空场检测 + 向后兼容）
### 1) 新的波次数据模型（向后兼容）
- 在 CSV 中新增“可选列”（旧表不填也能跑）：
  - `AllowMixedWave`（bool，可由关卡/线上热更开关覆写）
  - `MixedGroupId`（string/int，用于把“小怪段 + Boss段”归为同一混合波）
  - `Phase`（Small/Boss，可选；不填则用启发式：Prefab 是否包含 `EnemyMiniBoss`/名字含 `_Boss`）
  - `SpawnCount`（int，可选；不填则沿用 KeepSeconds + GenerateDuration 的时间制刷新）
  - `MaxWaitAfterSpawnSeconds`（保留，默认 3s；用于“全部刷新完后进入下一波的最大等待时间”）
- 兼容策略：
  - 旧 CSV 没有 MixedGroupId/Phase/SpawnCount 时：按“单段波”处理（与当前行为一致），仅额外套用新的切波状态机。

### 2) 波次控制器状态机（Waiting／SpawningSmall／SpawningBoss／Completed）
- 新增 `WaveController`（替换/包裹现有 EnemyGenerator 的 Update 逻辑）：
  - `Waiting`：准备/进入波次（UI 更新、计数清零等）
  - `SpawningSmall`：只负责小怪刷新（按 count 或 duration 生成）
  - `SpawningBoss`：只负责 Boss 生成（一次性或按配置）
  - `Completed`：波次完成（触发下一波）
- 小怪死亡事件实时触发：
  - 引入 `WaveSpawnedMarker` 组件：在生成时挂到实例上，记录所属波次/阶段（Small/Boss），并在 `OnDestroy` 抛出事件给控制器（不侵入 Enemy/EnemyMiniBoss 逻辑也能计数）。

### 3) 混合波策略（A/B 二选一，运行时可切换）
- 方案 A（完全移除混合波）：
  - Loader 在加载阶段把同一 MixedGroupId 的 Small 段与 Boss 段拆成两个独立波次（保证 Boss 永远是单独波）。
- 方案 B（保留混合概念，强制无缝）：
  - `SpawningSmall` 结束条件：小怪“已完成刷新”且“小怪存活计数归零” → 立刻进入 `SpawningBoss` 并提交 Boss 生成请求。
  - 取消人为等待间隔：Boss 生成不依赖 `GenerateDuration`，只依赖“可生成”条件。

### 4) 空场检测机制（0.5 秒无敌人则强制提交 Boss 生成）
- 在 `SpawningSmall` 阶段：当小怪计数器归零且 Boss 未生成时，启动 0.5s 空场计时。
- 若 0.5s 内场景仍无敌人（Small/Boss 都为 0），立即提交 Boss 生成请求（并带重试/兜底），杜绝“干等 Boss”。

### 5) 全部刷新完后的最大时间（MaxWaitAfterSpawnSeconds）
- 刷新阶段结束后（达到 SpawnCount 或 KeepSeconds）：进入“后置等待窗口”，最多等待 MaxWaitAfterSpawnSeconds。
- 等待窗口内：
  - 若场景清空 → 立即 Completed
  - 若超时仍未清空 → 强制 Completed（避免拖波）

## （2）修复第 19 波 Boss 攻击异常（HitBox/HurtBox、事件流、GameOver、日志、测试）
### 1) 统一命中/伤害入口（确保一次命中一次伤害）
- 定位并统一所有玩家受伤来源：
  - 玩家 HurtBox（近战/碰撞）
  - BossProjectile（弹幕）
  - 旋转攻击等技能（例如 SpinAttackSkill）
- 新增 `PlayerDamageSystem`（或 `Player.ApplyDamage(DamageContext)`）：
  - 命中去重：同一帧同一伤害源只结算一次
  - 无敌帧：命中后进入 invincible window
  - 护盾：优先消耗护盾再扣血

### 2) 校正受击反馈事件流（按你要求的顺序）
- 命中 → 玩家减血 → 播放指定受击音效（对齐音效表 ID，如 `Sfx.HURT`）→ 屏幕震动（CameraController.ShakeCamera）→ UI 血条刷新（沿用 BindableProperty 刷新）
- 替换当前 BossProjectile 的命中判定为 HurtBox/Player 兼容方式，并让它走统一伤害入口（避免各处各扣各的）。

### 3) HP≤0 强制 GameOver + 阻断后续逻辑 + 战报日志
- 抽出 `Player.GameOver()`：
  - 进入 GameOver 后禁止任何治疗/护盾增长（在 Global/道具执行处加 guard）
  - 记录战报：BossID、伤害源、死亡帧号（Time.frameCount），并统一输出/持久化（Debug + 可选保存）

### 4) 边界测试与 50 次回归
- 增加 PlayMode 测试覆盖：无敌帧、护盾抵消、多段伤害同帧命中、0 血即 GameOver。
- 增加“50 次连续回归”自动化测试：同一测试用例循环 50 次模拟命中链路，断言 0 血必定进入 GameOver，复现率 0。

## （3）炸弹与经验收集道具视觉引导（脉冲环 + 屏外箭头 + 性能 + 开关）
### 1) 掉落瞬间脉冲环特效 + 3D 空间音效
- 在 `Global.GeneratePowerUp*`（掉落生成点）挂钩：对 Bomb/Exp 掉落时生成 `PulseRing` 特效（1.2s 淡出）。
- 新增一个简单的发光扩散 Shader + Material + Prefab（当前工程无 shader 文件，需要新增）。
- 3D 音效：使用临时 AudioSource（spatialBlend=1，距离衰减），复用现有音频资源（例如 Bomb/Exp 提示音），或新增专用低频提示音并纳入资源管理。

### 2) 屏外箭头引导
- 新增 `LootGuideSystem`：
  - 追踪 Bomb/Exp 掉落对象位置
  - 当对象在屏幕外且玩家距离≥8m：在屏幕边缘生成箭头
  - 箭头颜色绑定稀有度（需定义掉落稀有度字段/映射表）
  - 箭头大小随距离线性增大，封顶为屏幕高度 8%
  - 0.8s 闪烁周期
- 性能控制：对象池 + 最多 16 个箭头 + 每帧无 GC 分配（缓存 Camera/RectTransform 计算）。

### 3) 开关配置与玩家设置项
- 增加 `EnableLootGuide`（全局配置）与 UI 设置项（可完全关闭），持久化到存档/PlayerPrefs。

## 单元测试（≥10 条，100% 通过）
- WaveStateMachine：
  - 正常混合波（B）从 Small→Boss→Completed
  - A 策略拆波后顺序正确
  - 提前清场触发 Boss 立即生成
  - 空场 0.5s 触发 Boss 生成请求
  - MaxWaitAfterSpawnSeconds 超时强制 Completed
  - “丢包补偿”模拟：Spawn 请求失败 N 次后重试成功（用 FakeSpawner）
  - 延迟刷新：SpawnInterval 大于 0，计时正确
- BossDamage：
  - 同帧多段伤害只结算一次
  - 无敌帧生效
  - 护盾抵消优先级
  - 0 血必 GameOver（并验证战报日志字段）

## 交付物
- 波次系统文档（配置表 + 调用链 + 状态机图）
- 新波次系统代码（可配置 A/B + AllowMixedWave + 向后兼容）
- 19 波 Boss 攻击链路修复（统一伤害入口 + 事件流 + GameOver + 战报）
- Loot 引导系统（脉冲环 Shader + 屏外箭头 + 配置开关）
- ≥10 条自动化测试 + 50 次回归测试用例