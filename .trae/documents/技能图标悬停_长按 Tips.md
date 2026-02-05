## 需求理解
- 左下角“已解锁技能图标条”（UnlockedIconPanel 动态生成的图标）在鼠标悬停时显示技能介绍。
- 移动端用“手长按”触发同样的介绍弹窗，松开/移出后隐藏。
- 弹窗位置在图标上方，且不遮挡交互（不应吃掉射线）。

## 现状调研结论
- 左下角图标由 [UnlockedIconPanel.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/UI/UIGamePanel/UnlockedIconPanel.cs) 在 Awake 里根据 ExpUpgradeSystem.Items 动态实例化 Image。
- HUD 预制体里已经有现成的 Tip 节点（含 Title/Description 文本），但目前没有任何脚本驱动它显示/隐藏或填充内容：见 [UIGamePanel.prefab:Tip](file:///d:/unity/Vampire%20Survivor-like/Assets/Art/UIPrefab/UIGamePanel.prefab#L1237-L1314)。
- ExpUpgradeItem 的 Description 当前是“下一等级描述”（CurrentLevel+1），需要补一个“当前等级描述”以用于已解锁图标的介绍：见 [ExpUpgradeItem.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/System/ExpUpgrade/ExpUpgradeItem.cs)。

## 方案（最小侵入，复用现有 Tip UI）
1) 新增 Tooltip 视图脚本（不改 Prefab 也能跑）
- 新增 `UITooltipView`（MonoBehaviour，命名空间 `VampireSurvivorLike`）
  - 运行时找到/绑定 Tip 下的 `Title`、`Description` Text；提供 `ShowFor(RectTransform target, string title, string desc)` / `Hide()`。
  - 负责把 Tip 定位到目标图标上方：以目标 RectTransform 的上边缘中心为基准，加一个 Y 偏移；并对 Canvas 边界做 clamp，避免跑出屏幕。
  - 运行时确保 Tip 不阻挡射线：给 Tip 加 CanvasGroup 并设 `blocksRaycasts=false`，同时把 Tip 及子节点 Graphic 的 `raycastTarget=false`。

2) 新增 Tooltip 触发脚本（hover + 长按）
- 新增 `TooltipTrigger`（MonoBehaviour，实现 `IPointerEnterHandler / IPointerExitHandler / IPointerDownHandler / IPointerUpHandler`）
  - PC：`OnPointerEnter` 立即显示；`OnPointerExit` 隐藏。
  - 移动端：`OnPointerDown` 开始计时（例如 0.35s），到点才显示；`OnPointerUp/Exit` 取消计时并隐藏。
  - 触发器只负责“何时显示/隐藏”，不负责“怎么摆放”，显示时调用 `UITooltipView.ShowFor(...)`。

3) 让 UnlockedIconPanel 在生成图标时挂上触发器并注入数据
- 修改 [UnlockedIconPanel.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/UI/UIGamePanel/UnlockedIconPanel.cs)
  - 图标实例化后：给 `self.gameObject` AddComponent<TooltipTrigger>()，设置目标为该图标的 RectTransform，title/desc 来自对应 `ExpUpgradeItem`。
  - 在“超武解锁替换图标”分支（如 `simple_knife` → `PairedIconName`）里，同步更新 trigger 的 title/desc 为 `PairedName/PairedDescription`（若存在）。

4) 补齐 ExpUpgradeItem 的“当前等级描述”能力
- 修改 [ExpUpgradeItem.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/System/ExpUpgrade/ExpUpgradeItem.cs)
  - 增加 `GetDescriptionAtLevel(int level)` 或 `CurrentDescription` 属性（用 `_mDescriptionFactory(level)`），以便 Tooltip 展示“当前等级效果”。
  - Tooltip 默认展示：`Name + " Lv." + CurrentLevel`，描述用 `CurrentDescription`（如果 level==0 则用下一等级或基础描述，按现有工厂逻辑兜底）。

5) 在 UIGamePanel 初始化时确保能拿到 Tip 视图
- 修改 [UIGamePanel.cs](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/UI/UIGamePanel.cs)
  - `OnInit` 时 `transform.Find("Tip")`，若存在则给 Tip 根节点挂 `UITooltipView`（若已挂则复用），并默认 `Hide()`。
  - （兜底）如果场景里没有 EventSystem（UIKit 通常会创建），则运行时创建一个 EventSystem + StandaloneInputModule，保证指针事件可用。

## 验证方式
- Editor：进入游戏，升一级获得技能图标后，将鼠标移到左下角图标上，Tip 在图标上方显示并随移出隐藏。
- Editor：按住鼠标左键不松（模拟长按），超过阈值显示，松开即隐藏。
- 超武：触发“配对升级后替换图标”的那几项（knife/sword/bomb 等），图标变化后 Tip 文案也同步变化。

## 影响范围与文件清单
- 新增：`Assets/Scripts/UI/UITooltipView.cs`、`Assets/Scripts/UI/TooltipTrigger.cs`（或放在 `UI/UIGamePanel/` 同目录以符合现有结构）。
- 修改：`Assets/Scripts/UI/UIGamePanel/UnlockedIconPanel.cs`、`Assets/Scripts/System/ExpUpgrade/ExpUpgradeItem.cs`、`Assets/Scripts/UI/UIGamePanel.cs`。

如果你确认这个方案，我就按上述步骤开始落地实现。