# 项目结构整理规范

## 目标

- 保持运行时代码、系统代码、UI 代码边界清晰，降低后续维护成本。
- 避免把临时产物、构建产物混入源码目录。
- 为后续功能迭代提供统一落位规则。

## 根目录分层

- `Assets/`：游戏源资源与源码（唯一核心开发目录）。
- `Packages/`：Unity 包配置与本地包。
- `ProjectSettings/`：Unity 项目设置。
- `Docs/`：项目文档与设计说明。
- `Tools/`：开发/运维脚本（不参与运行时逻辑）。
- `Tests/`：独立测试项目与测试结果。
- `AssetBundles/`：打包产物（按需维护，非日常逻辑改动目录）。
- `Build/ Library/ Temp/ Logs/ obj/ UserSettings/`：生成目录，不放业务代码。

## 代码目录规范（Assets/Scripts）

- `Global/`：全局状态与全局配置入口（例如 `Global.cs`）。
- `Game/`：战斗核心循环与对象行为（玩家、敌人、掉落、生成器等）。
- `System/`：跨模块系统（升级、成就、存档、性能、排行榜等）。
- `UI/`：所有 UI 面板与 UI 控制器（含 `*.Designer.cs`）。
- `Config/`：运行时配置加载器与配置映射。
- `Localization/`：本地化运行时逻辑与组件。

## 文件落位规则

- 新增 `UI*` 脚本放到 `Assets/Scripts/UI/` 或其子目录。
- 新增 `*System` 脚本放到 `Assets/Scripts/System/` 对应子模块。
- `*.Designer.cs` 与对应主脚本放在同一目录。
- Editor-only 脚本放 `Assets/Editor/`，不要混入运行时代码目录。
- CSV/本地化运行时数据放 `Assets/StreamingAssets/Config` 与 `Assets/StreamingAssets/Localization`。

## 本次整理（2026-03-05）

- `UIGameLocalLeaderboardPanel.cs/.Designer.cs` 从 `Assets/Scripts/Game/` 归位到 `Assets/Scripts/UI/UIGamePanel/`。
- `Enemy.Designer.cs` 从 `Assets/Scripts/Game/` 归位到 `Assets/Scripts/Game/Enemy/`。

## 巡检方式

- 使用脚本：`Tools/Maintenance/Inspect-ProjectStructure.ps1`
- 命令示例：
  - `powershell -ExecutionPolicy Bypass -File Tools/Maintenance/Inspect-ProjectStructure.ps1`
  - 严格模式：`powershell -ExecutionPolicy Bypass -File Tools/Maintenance/Inspect-ProjectStructure.ps1 -Strict`
