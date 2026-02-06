## 问题分析
### (1) 成就列表不随语言切换
- 成就条目文本来源于 [AchievementSystem.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/System/Achievement/AchievementSystem.cs) 的 `WithName/WithDescription`，目前全部是中文硬编码。
- 列表在 [AchievementPanel.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/UI/UIGameStartPanel/AchievementPanel.cs) 的 `Awake()` 里只生成一次，之后切换语言不会触发重建/刷新。

### (2) “返回主菜单”文字太长
- 该按钮文案来自 `ui.settings.return_main_menu`（在 `core.zh-Hans.csv` 里是“返回主菜单”），改成“主菜单”即可。

### (3) 首次进入 Game 场景出现 game.ui.level/wave 等 key
- `UIGamePanel` 虽然在 [UIGamePanel.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/UI/UIGamePanel.cs#L15-L23) 里 `PreloadTable("game")`，但面板初始化时就立刻 `Format/T` 写入文本；此时 game 表尚未异步加载完成，`T()` 会回退返回 key 本身。
- 当你在设置面板切换语言时，会触发 `ReadyChanged`/重刷逻辑，因此这些文本才“恢复正常”。

## 修复方案
### A. 成就列表支持中英文切换
1. 改造 [AchievementItem.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/System/Achievement/AchievementItem.cs)
   - 新增 `NameKey/DescriptionKey`（并提供 `WithNameKey/WithDescriptionKey`）。
   - 新增读取用的 `DisplayName/DisplayDescription`：优先用 key 走 `LocalizationManager.T`，否则回退旧的 `Name/Description`。
2. 修改 [AchievementSystem.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/System/Achievement/AchievementSystem.cs)
   - 每条成就改为设置 `NameKey/DescriptionKey`（例如 `achievement.3_minutes.name/achievement.3_minutes.desc`）。
3. 修改 [AchievementPanel.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/UI/UIGameStartPanel/AchievementPanel.cs)
   - 把生成列表逻辑抽成 `RefreshList()`。
   - 监听 `LocalizationManager.ReadyChanged`（或语言切换事件）在面板显示时重刷列表文本。
   - “已完成”后缀改为本地化 key（例如 `ui.achievement.completed`）。
4. 修改 [AchievementController.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/UI/UIGamePanel/AchievementController.cs)
   - 弹窗标题/描述也改用本地化（避免仍显示中文或 key）。
5. 在 `core.zh-Hans.csv/core.en.csv` 增加上述成就相关 keys（13 条 name + 13 条 desc + completed/达成提示等）。

### B. 设置界面“主菜单”文案
- 把 `core.zh-Hans.csv` 的 `ui.settings.return_main_menu` 值由“返回主菜单”改为“主菜单”。

### C. 首进 Game 场景 key 问题
- 修改 [UIGamePanel.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/UI/UIGamePanel.cs)
  - 新增 `LocalizationManager.ReadyChanged` 监听；在 **确认 game 表已可用** 后（用 `TryGet("game.ui.level")` 等探测）统一重刷所有 HUD 文本。
  - 保留原有 `CurrentLanguage.Register`，但额外保证“不切语言也会在表加载完成后自动刷新”。

## 验证方式
- 启动游戏进入开始界面：切换语言后，成就列表条目立即更新语言。
- 设置界面：按钮显示“主菜单”，不溢出。
- 首次进入 Game 场景：HUD 不再出现 `game.ui.*` key；即使出现也会在表加载完成后自动刷新为正确文本，无需手动切语言。