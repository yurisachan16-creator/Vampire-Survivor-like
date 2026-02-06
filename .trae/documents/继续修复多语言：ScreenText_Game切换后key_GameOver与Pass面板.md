## 问题定位
- **(1) ScreenText**：在 [UIGameSettingsPanel.prefab](file:///d:/unity/Vampire%20Survivor-like/Assets/Art/UIPrefab/UIGameSettingsPanel.prefab#L1590) 存在 `ScreenText`，初始值为“窗口化/全屏”，当前 [UIGameSettingsPanel.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/UI/UIGameSettingsPanel.cs) 的刷新逻辑未覆盖它。
- **(2) Game 场景切一次语言后出现 game.ui.xxx**：`UIGamePanel` 的文本来自 `game` 表，但 `LocalizationManager` 在语言切换时只保证加载 `core`，不会自动把之前 `PreloadTable("game")` 的请求“继承”到新语言缓存，导致切换后 game 表缺失，后续 `Format/T` 回退显示 key。
- **(3) GameOver/Pass 面板**：两个面板 prefab 内文字为硬编码（例如“游戏结束/游戏通关/回到主页”），对应脚本未绑定本地化刷新。

## 修改方案
### 1) 设置界面 ScreenText 本地化
- 在 [UIGameSettingsPanel.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/UI/UIGameSettingsPanel.cs) 的 `refreshUiText()` 中获取 `DisplaySettings/ScreenText` 的 `Text` 并赋值 `LocalizationManager.T("ui.settings.screen_mode")`，并注册字体。
- 在 `core.zh-Hans.csv`/`core.en.csv` 增加 `ui.settings.screen_mode`（中文“窗口化/全屏”，英文“Windowed/Fullscreen”）。

### 2) 修复 Game 场景切语言后显示 key
- 在 [LocalizationManager.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/Localization/LocalizationManager.cs) 增加全局 `HashSet<string>` 记录“被请求 preload 的表名”。
  - `PreloadTable(table)`：把 `table` 写入集合，并继续按当前逻辑加载当前语言的表。
  - `ActivateLanguage(language)`：在 `core` 加载完成后，遍历该集合，把所有表（除 core）对 **新语言** 执行 `LoadTableForLanguage`，确保切语言后 game/upgrade/ability 等表也会加载。
- 在 [UIGamePanel.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/UI/UIGamePanel.cs) 保留现有 `ReadyChanged`/`CurrentLanguage` 的刷新，但把若干直接写 `Format()` 的回调加“表未就绪则跳过”的保护，避免切换瞬间写入 key（随后会在表加载完成时统一刷新）。

### 3) GameOver 与 GamePass 面板本地化
- 在 [UIGameOverPanel.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/UI/UIGameOverPanel.cs) / [UIGamePassPanel.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/UI/UIGamePassPanel.cs) 中：
  - 获取标题 Text（例如 prefab 中的“游戏结束/游戏通关”）与按钮文本（`BtnBackToStart` 的子 Text）。
  - 监听 `LocalizationManager.ReadyChanged` 并在 `IsReady` 后刷新文本。
  - 按钮文案可复用 `ui.settings.return_main_menu`（当前中文为“主菜单”），标题用新 key：`ui.gameover.title` / `ui.gamepass.title`。
- 在 `core.zh-Hans.csv`/`core.en.csv` 增加上述 title key。

## 验证方式
- 进入设置界面：`ScreenText` 在中英文下正确显示。
- 进入 Game 场景：切换语言一次后 HUD 仍保持正确翻译，不出现 `game.ui.*`。
- 触发 GameOver/GamePass：面板标题与按钮文本随语言切换正确变化。