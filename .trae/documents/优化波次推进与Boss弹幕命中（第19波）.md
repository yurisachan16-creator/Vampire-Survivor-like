## 现状与问题定位
- “小怪混 Boss”出现空场等待，主要来自两类机制叠加：
  - 波次开始后第一次生成要等 `GenerateDuration` 秒（因此 Boss 波开始时会空等）。见 [EnemyGenerator.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/Game/EnemyGenerator.cs)
  - 目前波次结束与“清怪推进”缺少对“本波还会不会继续刷”的明确阶段划分，导致空场时不知该刷/该切波。
- 第19波 Boss 子弹不伤害玩家：`BossProjectile.OnTriggerEnter2D` 用 `other.CompareTag("Player")`，但实际发生碰撞的通常是玩家子物体 HurtBox（未必带 Player 标签），因此命中回调不触发扣血/销毁。见 [BossProjectile.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/Game/Enemy/BossSkills/BossProjectile.cs)、[Player.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/Game/Player.cs)

## 改造目标（对应你的 3 点）
1) 场景怪清空时，能可靠进入下一波；且在“小怪→Boss”过渡时不出现空等。
2) 波次生成逻辑可配置：波次列表、生成持续时间、刷新间隔、刷完后最多等待多久切下一波。
3) 第19波 Boss 弹幕稳定命中玩家并触发正确的扣血/死亡逻辑。

## 实施方案
### A. 波次逻辑：引入“生成阶段/清理阶段”并补齐配置项
- 扩展波次数据（ScriptableObject + CSV）：
  - 现有：`GenerateDuration`=刷新间隔、`KeepSeconds`=生成持续时间（注释也是这个含义）。见 [LevelConfig.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/Config/LevelConfig.cs)
  - 新增：`PostSpawnMaxWaitSeconds`（刷完后最多等待清怪多久进入下一波）。
    - CSV 末尾追加一列（保持向后兼容：缺失则使用默认值）。见 [EnemyWaveConfigLoader.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/Config/EnemyWaveConfigLoader.cs)
- 修改 EnemyGenerator：
  - 波次开始时“立即生成一次”（解决 Boss 波开始空等）。做法：开始新波时直接调用一次 Spawn（或将计时器初始化为 `GenerateDuration` 以便下一帧生成）。
  - 生成阶段：按间隔生成，直到 `KeepSeconds`（生成持续时间）到点。
  - 清理阶段：停止生成；当场景中 `Enemy` 与 `EnemyMiniBoss` 都为 0 即进入下一波；若超过 `PostSpawnMaxWaitSeconds` 仍未清完也强制进入下一波（防止卡关）。
  - 小怪混 Boss “空场但还没到下一次刷新”的情况：
    - 我会在生成阶段加入一个“空场触发器”：若场景已空并且距离下一次生成还有明显空档，则**优先推进下一波**（让 Boss 波尽快开始），而不是让玩家空等。这个阈值会做成一个常量或可配置字段（默认 0.5~1s）。

### B. 第19波 Boss 子弹：用 Player 根对象判定命中 + 统一伤害入口
- 修改 [BossProjectile.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/Game/Enemy/BossSkills/BossProjectile.cs)：
  - 命中判定改为 `other.GetComponentInParent<Player>() != null`（不依赖标签在子碰撞体上）。
  - 命中后调用玩家的统一受伤方法并销毁子弹。
- 为避免“子弹扣血但不会触发 GameOver/停止走路音效”等不一致，修改 [Player.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/Game/Player.cs)：
  - 抽一个 `public void TakeDamage(int amount)`，复用现有受伤/死亡 UI 流程。
  - 玩家被敌人碰撞与被 Boss 弹幕命中都走同一入口。

## 配置落地（CSV/ScriptableObject）
- 在 CSV 新增列后，我会先给“Boss 波/关键波”填合理的 `PostSpawnMaxWaitSeconds`（例如 Boss 波给 30~60s，小怪波给 5~10s），确保行为符合“刷完→清怪→切波”的预期。
- 如果你更希望某些波“清空就立刻切波”，我会用更小的 PostSpawnMaxWaitSeconds 或开启上面的空场阈值推进。

## 验证
- 编译诊断无新增错误。
- 运行验证：
  - 小怪清完后不会空等，Boss 波会立刻出 Boss（或至少不再等一个刷新间隔）。
  - 刷完后在 `PostSpawnMaxWaitSeconds` 内清怪会立刻切下一波；超时也能强制切波不致卡死。
  - 第19波 Boss 弹幕能稳定命中玩家、扣血、并在 HP<=0 时进入 GameOver。