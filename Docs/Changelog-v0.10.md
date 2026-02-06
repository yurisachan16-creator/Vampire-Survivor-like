# Changelog - v0.10（Feature-编辑器制作）

本文件整理分支 `Feature-编辑器制作` 的 v0.10 全量改动（按提交顺序），用于回溯版本演进与定位关键代码/文档入口。

## 版本线

| 版本 | 提交 | 提交说明 | 主要内容摘要 |
|---|---|---|---|
| v0.10.0 | 4f753339 | 数值平衡与生成器改进 | 第一轮数值与刷怪节奏调整；为后续表驱动/生成器重构铺垫 |
| v0.10.1 | e53629d8 | 修复升级弹窗问题 | 修复升级面板弹出/恢复节奏等问题，避免异常弹窗或卡住 |
| v0.10.2 | 2523b763 | 敌人波次配置扩展&技能属性配置系统 | EnemyWaveConfig 扩展字段；AbilityConfig CSV 加载与应用链路落地 |
| v0.10.3 | 43e493aa | 重新命名Enmey以及Sprite贴图，重构MiniBoss | 资源/命名整理；MiniBoss 结构化重构（BossType/技能组合/FSM） |
| - | dcc2d393 | Merge: 解决.meta文件冲突 | Unity .meta 冲突处理，稳定资源引用 |
| v0.10.4 | 53113c5f | 修复MiniBoss脚本相关Bug&敌人波次生成优化 | MiniBoss 修复；波次生成从“逐行”向“组/段落/混合波”优化 |
| v0.10.5 | fa9f2f95 | 修复SimpleSword对Boss不生效 | SimpleSword 命中筛选/碰撞判定修正，使其对 Boss 生效 |
| v0.10.6 | b3458d3f | 制作敌人属性配置系统 | EnemyStatsConfig（Resources 单例）+ 自定义 Inspector（批量/导入导出） |
| v0.10.7 | 4560cdcf | 清怪后立即切波&修复bug | 清怪切波更及时；加入等待上限等兜底，减少拖波与干等 |
| v0.10.8 | 5fd16412 | 战斗系统&拾取引导更新 | 掉落物引导（屏外箭头/脉冲环/3D 音效）+ 可开关；战斗反馈链路完善 |
| v0.10.9 | 079d3338 | 技能图标 Tips（悬停/长按显示介绍） | Tooltip 系统（PC 悬停/移动端长按）用于技能图标说明 |
| v0.10.10 | e2ee7ac1 | 初步实现多语言1 | i18n 系统分阶段落地（表结构/运行时 API/基础接入） |
| v0.10.11 | 936f673d | 初步实现多语言2 | i18n 完善（更多 UI 接入/工具链补全） |
| v0.10.12 | fcdb0995 | 初步实现多语言3 | i18n 成型（缓存/调试/导入导出/测试用例等） |

## 按模块归纳

### 表驱动：波次配置（EnemyWaveConfig）

- 数据源：`Assets/StreamingAssets/Config/EnemyWaveConfig.csv`
- 加载/解析：`Assets/Scripts/Config/EnemyWaveConfigLoader.cs`
- 生成逻辑与混合波控制：`Assets/Scripts/Game/EnemyGenerator.cs`
- 现状说明文档：`Assets/Docs/WaveSystem.md`

关键能力：
- CSV 字段扩展：`MaxWaitAfterSpawnSeconds`、掉落概率、`AllowMixedWave/MixedGroupId/Phase/SpawnCount` 等
- “波次组/段落”结构：小怪段落与 Boss 段落分离；支持混合波策略与清怪后推进

### 表驱动：技能配置（AbilityConfig）

- 数据源：`Assets/StreamingAssets/Config/AbilityConfig_i18n.csv`（优先），回退 `AbilityConfig.csv`
- 加载/缓存：`Assets/Scripts/Config/AbilityConfigLoader.cs`
- 生效入口：`Assets/Scripts/Global/Global.cs` 的 `ApplyAbilityConfig()`

### 配置系统：敌人属性（EnemyStatsConfig）

- 配置资产：`Assets/Scripts/Config/EnemyStatsConfig.cs`（Resources 单例：`Resources/EnemyStatsConfig`）
- 编辑器 Inspector：`Assets/Editor/EnemyStatsConfigEditor.cs`

关键能力：
- 集中管理 BaseHP/BaseSpeed/BaseDamageMultiplier 与掉落概率等
- 可视化批量编辑、排序、搜索、CSV 导入导出

### 编辑器工具链

- CSV+Prefab 校验：`Assets/Editor/EnemyConfigValidator.cs`
- 本地化导入导出工作台：`Assets/Editor/Localization/LocalizationWorkbenchWindow.cs`

### Boss 体系（MiniBoss）

- Boss 结构：`Assets/Scripts/Game/Enemy/EnemyMiniBoss.cs`
- 技能实现：`Assets/Scripts/Game/Enemy/BossSkills/*`

关键能力：
- BossType 决定技能组合（冲刺/弹幕/召唤/狂战/混合）
- FSM 驱动“追踪/释放技能”的行为流

### 体验：掉落物引导（Loot Guide）

- 开关持久化：`Assets/Scripts/Global/GameSettings.cs`（PlayerPrefs）
- 引导系统实现：`Assets/Scripts/Game/GameplayObject.cs`（LootGuideSystem）

### 体验：技能图标 Tooltip

- 触发器：`Assets/Scripts/UI/TooltipTrigger.cs`（PC 悬停 / 移动端长按）
- Tooltip 视图：`Assets/Scripts/UI/UITooltipView.cs`
- 业务接入：`Assets/Scripts/UI/UIGamePanel/UnlockedIconPanel.cs`

### i18n（多语言）

- 生产版说明：`Docs/I18N.md`
- 运行时核心：`Assets/Scripts/Localization/LocalizationManager.cs`
- 表结构：`Assets/StreamingAssets/Localization/manifest.json` 与 `{table}.{lang}.csv`
- 编辑器工作流：`Assets/Editor/Localization/LocalizationWorkbenchWindow.cs`
- 测试：`Assets/Tests/EditMode/LocalizationCsvTests.cs`、`Assets/Tests/PlayMode/LocalizationSwitchTests.cs`

