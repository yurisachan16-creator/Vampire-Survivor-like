## 目标与结论

* 以 `*.csv` 为唯一生效配置源；`Docs/配置表.xlsx` 仅为说明文档，不作为更新目标。

* 本次工作聚焦：用 git 对比拿到 Enemy 资源命名变更 → 系统更新所有 CSV 里的引用 Key（尤其 `EnemyPrefabName`）→ 同步更新 `Enemy Prefab Mapping.asset` 的 Name→Prefab 映射 → 做完整配置校验，确保生成/显示/动画链路可用。

## 1) 用 git 对比提取 Enemy 命名变更清单

* 执行 git diff/rename 对比：重点目录

  * `Assets/Art/Prefab/Enemy`（Prefab 文件/对象命名变更）

  * `Assets/Art/Sprite`/动画相关目录（如有按名称加载的情况）

  * 以及所有被改名资源的 `.meta`（确认是 rename 还是“新建+删除”）

* 产出一份“旧名→新名”的 Key 映射表（至少包含：Prefab 的旧名/新名）。

## 2) 盘点所有会受影响的配置引用点（只关注 CSV）

* 必查并会修改：

  * `Assets/StreamingAssets/Config/EnemyWaveConfig.csv`

* 产物同步（若仍被当作可运行产物保留）：

  * `Build/WebGL/StreamingAssets/Config/EnemyWaveConfig.csv`

* 全仓库扫描：grep 旧 Enemy 名称，确认没有其他 CSV/文本配置在引用旧名。

## 3) 更新资源 Key 与映射关系（核心修复）

* 更新 `Assets/Art/Config/Enemy Prefab Mapping.asset`

  * 以当前实际存在的 `Assets/Art/Prefab/Enemy/*.prefab` 为准，重建映射列表：`Name = prefab.name`，`Prefab = 对应 Prefab`。

  * 兼容策略（更稳，推荐默认启用）：同时保留旧 Key 和新 Key 两条映射（不同 Name 指向同一 Prefab），这样即使某些 CSV 行漏改也不会立即导致整波次失效。

* 更新 `EnemyWaveConfig.csv`

  * 将所有 `EnemyPrefabName` 统一替换为新命名规范的 Key。

  * 专项处理 `EnemyMiniBoss`：根据 git rename 记录与现存 Boss Prefab（如 `Enemy_*_Boss`）确定替换目标，确保 Boss 波次不再因为 Key 失配而被跳过。

## 4) 完整配置验证（可自动化、可复用）

* 静态校验（无需进游戏也能发现引用断裂）：

  * 读取 `EnemyWaveConfig.csv`，对每行 `EnemyPrefabName` 做 `PrefabMapping.HasPrefab(name)` 断言。

  * 对所有被引用 Prefab 断言：存在 `IEnemy` 实现组件（如 `Enemy`/`EnemyMiniBoss`），并包含用于显示的 `SpriteRenderer`；若该敌人依赖动画，再校验 Animator/AnimationClip 引用不为空。

* 可运行验证（在现有场景链路上验证）：

  * `Game.unity` 场景里 `UseCSVConfig=1`，用它做回归：敌人能生成、显示正常、Boss/普通怪动画正常播放。

  * 如仓库已有测试场景（`TestMaxEnemyCount` 等），一并跑一次验证极限生成。

## 5) 交付物

* git 对比得出的“旧→新”命名映射表（便于后续再改名时复用）。

* 已同步更新的 CSV 与 PrefabMapping 映射资产。

* 一份可重复执行的配置校验入口（EditMode 测试或校验脚本），用于后续防止再出现引用断裂。

