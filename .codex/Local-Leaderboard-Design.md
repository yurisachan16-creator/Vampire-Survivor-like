# 本地排行榜功能设计说明

## 1. 目标
实现一个本地排行榜（Top 20），记录每局关键数据，并在以下位置展示：
- 开始页：新增“排行榜”按钮，打开排行榜面板
- 结算页：`Game Over` 与 `Game Pass` 页面内展示 Top 20（可滚动）

## 2. 记录字段
每条记录包含：
- 综合评分 `Score`
- 生存时间（秒）`SurvivalSeconds`
- 波次（分钟）`WaveMinute`
- 等级 `Level`
- 金币 `Coins`
- 击杀数 `KillCount`（小怪 + Boss）
- 死亡原因 `DeathReason`（通关局固定为“通关”）
- 记录时间戳 `TimestampUnix`

## 3. 排序与保留规则
- 评分公式：`Score = 波次*5000 + 生存秒数*50 + 等级*200 + 击杀*1`
- 排序优先级（降序）：
  1. Score
  2. WaveMinute
  3. SurvivalSeconds
  4. Level
  5. KillCount
  6. TimestampUnix（新的优先）
- 仅保留 Top 20

## 4. 持久化方案
- 存储介质：`PlayerPrefs`
- Key：`Leaderboard.Top20.v1`
- 格式：JSON（`JsonUtility`）

## 5. 写入时机
- 失败结算：写入一次
- 通关结算：写入一次
- 需防重复写入（同一局仅一次）

## 6. 系统与代码改动

### 6.1 新增系统
- `Assets/Scripts/System/Leaderboard/LeaderboardSystem.cs`
  - 负责：加载、保存、写入、排序、裁剪、清空
  - 对外：
    - `GetTopEntries()`
    - `RecordCurrentRun(isClear, deathReason)`
    - `ClearAll()`
    - `OnLeaderboardChanged`

### 6.2 全局统计扩展
- `Assets/Scripts/Global/Global.cs`
  - 新增 `KillCount`
  - 在 `ResetData()` 中重置为 0

### 6.3 击杀计数
- `Assets/Scripts/Game/Enemy/Enemy.cs` 的 `Die()`：`KillCount++`
- `Assets/Scripts/Game/Enemy/EnemyMiniBoss.cs` 的 `OnDeath()`：`KillCount++`

### 6.4 结算写榜
- 失败：`Player.GameOver(...)` 触发写榜
- 通关：`UIGamePassPanel` 打开时写榜（死亡原因固定“通关”）

### 6.5 UI改动
- 开始页：
  - `UIGameStartPanel.prefab` 新增 `BtnLeaderboard`
  - `UIGameStartPanel.cs/.Designer.cs` 增加按钮引用与打开逻辑
- 新增排行榜面板：
  - `UILeaderboardPanel.prefab`
  - `UILeaderboardPanel.cs/.Designer.cs`
  - 含 `ScrollRect` + 列表项 + `BtnClear` + `BtnClose`
- 结算页：
  - `UIGameOverPanel.prefab`、`UIGamePassPanel.prefab` 内嵌 Top20 列表区域

## 7. 本地化文案
建议新增 key（`core` 表）：
- `ui.start.leaderboard`
- `ui.leaderboard.title`
- `ui.leaderboard.clear`
- `ui.leaderboard.empty`
- `ui.leaderboard.score`
- `ui.leaderboard.death_reason`
- `ui.leaderboard.confirm_clear_title`
- `ui.leaderboard.confirm_clear_desc`

## 8. 验收标准
- 每局结束后记录正确写入
- 排序与 Top20 裁剪正确
- 开始页和结算页都可查看榜单
- 清空功能生效并持久化
- 多语言文案正常显示
- 不影响现有成就、金币升级、结算流程

## 9. 风险与注意事项
- 需避免同一局重复写入
- JSON损坏时需容错（回退为空榜）
- UI列表建议做轻量复用，避免重复逻辑
