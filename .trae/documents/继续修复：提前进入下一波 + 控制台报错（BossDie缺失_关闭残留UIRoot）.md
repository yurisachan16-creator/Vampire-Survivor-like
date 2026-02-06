## 修复点 1：清怪后立即进入下一波
- 修改 [EnemyGenerator.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/Game/EnemyGenerator.cs) 的波次结束逻辑：
  - 新增一个“本波已生成过敌人”的标记（例如 `_hasSpawnedInCurrentWave`），在每次开始新波时置为 false，在成功 Instantiate 敌人时置为 true。
  - 在当前波次更新末尾增加提前结束条件：当 `_hasSpawnedInCurrentWave == true` 且 `EnemyGenerator.EnemyCount.Value == 0` 时，直接结束当前波（等价于把 `_mCurrentWave = null`、清空 `CurrentWaveName`、`WaveRemainingTime`），从而立即进入下一波。
  - 这样 [UIGamePanel.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/UI/UIGamePanel.cs) 现有“最后一波 && EnemyCount==0 && CurrentWave==null 通关”逻辑无需改动，最后一波清完也会立刻通关。

## 修复点 2：BossDie 音效缺失报错
- 修改 [EnemyMiniBoss.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/Game/Enemy/EnemyMiniBoss.cs) 的 `OnDeath()`：
  - 将 `AudioKit.PlaySound("BossDie")` 替换为工程内已存在的音效常量（推荐 `AudioKit.PlaySound(Sfx.ENEMYDIE)`，常量见 [QAssets.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/QFrameworkData/QAssets.cs)）。
  - 目的：彻底消除 `Not Find By ResSearchKeys: AssetName: bossdie`。

## 修复点 3：停止游戏时的 UIRoot 未清理告警
- 修改 [UIRoot.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/QFramework/Toolkits/UIKit/Scripts/UIRoot.cs)：
  - 增加“应用退出中”的静态标记（在 `OnApplicationQuit` / `OnDestroy` 设置），并在 `UIRoot.Instance` getter 里如果退出中则不再 `Instantiate(Resources.Load("UIRoot"))`。
  - 同时加 `Application.isPlaying` 防护：非运行态不创建 UIRoot，只返回场景中已存在的 UIRoot（若有）。
  - 目的：避免关闭/停止播放时因某些回调触发 `UIRoot.Instance`，导致临退出又创建一个 DontDestroyOnLoad 对象，从而出现“Some objects were not cleaned up… UIRoot”。

## 验证
- 重新跑一次编译诊断（确保无新增 C# 报错）。
- 逻辑验证点：
  - 当前波次清到 0 敌人后，下一波立即开始（不再等 KeepSeconds）。
  - Boss 死亡不再出现 bossdie 缺失报错。
  - 停止播放/关闭游戏时，UIRoot 残留告警显著减少或消失。