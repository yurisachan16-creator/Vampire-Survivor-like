这是一个 Unity 2022.3 LTS 的 2D 类 Vampire Survivors 项目（URP），项目名 VampireSurvivorLike。

请先阅读这些入口文件再开始工作：
1) Assets/Scripts/Global/Global.cs
2) Assets/Scripts/Game/GameStartController.cs
3) Assets/Scripts/Game/GameUIController.cs
4) Assets/Scripts/Game/EnemyGenerator.cs
5) Assets/Scripts/UI/UIGamePanel.cs
6) Assets/Scripts/Localization/LocalizationManager.cs

架构特征：
- 使用 QFramework（Architecture + UIKit + BindableProperty + ResKit + AudioKit）
- 游戏状态集中在 Global（大量 BindableProperty）
- 战斗波次是 CSV/ScriptableObject 双模式，核心在 EnemyGenerator + WaveController
- 局内升级：ExpUpgradeSystem；局外永久升级：CoinUpgradeSystem；成就：AchievementSystem
- 本地化是自研 CSV 运行时方案（StreamingAssets/Localization），不是纯 Unity Localization Runtime API
- 有性能模块：对象池、敌人注册表、空间索引、PC Instanced Renderer、Performance HUD

关键数据路径：
- 波次配置：Assets/StreamingAssets/Config/EnemyWaveConfig.csv
- 技能配置：Assets/StreamingAssets/Config/AbilityConfig_i18n.csv
- 文本本地化：Assets/StreamingAssets/Localization/*.csv

约束：
- 优先保持现有 QFramework 代码风格和 partial + Designer 结构
- 变更前评估是否影响 WebGL/Android（有平台分支逻辑）
- 非必要不要动 Build、Library、Temp、AssetBundles 产物目录
- 新增/移动脚本优先遵守 `Docs/Project-Structure.md` 的目录边界规则
- `UI*` 脚本放 `Assets/Scripts/UI`，`*System` 脚本放 `Assets/Scripts/System`
