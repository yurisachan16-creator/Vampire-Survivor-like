# TapTap 上架执行指南

本指南整理了当前仓库里已经落地的 TapTap 发布入口、文档和脚本，目标是先完成预约/关注页，再补 Android 与 Windows 包体。

## 当前仓库默认值

- Unity：`2022.3.62f2`
- 发布基线版本：`0.12.0`
- Android `applicationId`：`com.obsidian.vampiresurvivorlike`
- Standalone `applicationId`：`com.obsidian.vampiresurvivorlike`
- Android `versionCode`：`12`
- Android Target Architectures：`ARM64`
- Windows 启动相对路径：`Vampire Survivor-like.exe`

> 注意：正式游戏名尚未冻结。对外公开商店页前，不要使用工程代号 `Vampire Survivor-like` 作为公开标题。

## Unity 编辑器入口

已新增以下菜单：

- `Tools/TapTap/Apply Android Release Settings`
- `Tools/TapTap/Apply Windows Release Settings`
- `Tools/TapTap/Validate Release Readiness`
- `Tools/TapTap/Open Release Docs`

建议顺序：

1. 先执行 Android 或 Windows 的发布设置应用菜单。
2. 执行 `Validate Release Readiness` 确认关键配置。
3. 构建完成后再运行对应平台的整理脚本。

## 仓库内文档

- `Docs/TapTap-Store-Draft.md`
- `Docs/TapTap-Store-Assets-Checklist.md`
- `Docs/TapTap-Privacy-Policy-Template.md`
- `Docs/TapTap-Submission-Checklist.md`
- `Docs/TapTap-Windows-Package-Guide.md`

## 命令行脚本

- `Tools/Release/Test-TapTapReleaseConfig.ps1`
  - 校验 `ProjectSettings.asset` 中的版本号、包名、Target SDK、ARM64 配置
  - `-RequireKeystore` 会额外检查 keystore 和 alias 是否填写
- `Tools/Release/Prepare-TapTapWindowsPackage.ps1`
  - 从 `Build/Windows` 提取纯净文件
  - 默认输出 `.zip`
  - 会自动剔除 `DoNotShip` 调试目录

## 发布顺序

1. 完成 TapTap 预约/关注页草稿
2. 冻结正式游戏名与主视觉
3. 提审 Android 正式包
4. 提审 Windows 纯净包

## 外部后台说明

- 预约/关注页阶段只填商店资料，不上传正式下载包
- Windows 包建议保持解压即玩，不做安装器优先
- Android 发布包必须使用正式签名，不要提交 debug 包
