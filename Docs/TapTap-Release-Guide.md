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
- 正式中文名：`夜幕幸存者`
- 繁體中文名：`夜幕倖存者`
- 英文名：`Nightfall Survivors`
- 日文名：`宵闇サバイバーズ`
- 韩文名：`암야의 생존자들`
- 联系邮箱：`yurisachan16@gmail.com`

> 对外公开商店页、素材和隐私政策时，统一使用上述正式名称，不再使用工程代号 `Vampire Survivor-like`。

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

## 地区与语言发布策略

仓库当前支持语言来自 [manifest.json](/d:/unity/Vampire%20Survivor-like/Assets/StreamingAssets/Localization/manifest.json)：`zh-Hans / zh-Hant / en / ja / ko`。

### 国区

- 分发范围：中国大陆
- 主导语言：简体中文
- 页面标题：`夜幕幸存者`
- 简中是国区页面的唯一主展示语言

### International

- 分发策略：全球预约页
- 主文案语言：English
- 优先补充语言：繁體中文、英文、日文、韩文
- 简中主要用于国区页面，不作为 International 主展示语言

### `language_region_priority`

| 语言 | 国家/地区 | 备注 |
| --- | --- | --- |
| `zh-Hant` | 台湾、香港、澳门 | 繁中页面与封面优先覆盖 |
| `en` | 美国、加拿大、英国、澳大利亚、新西兰、新加坡、菲律宾、马来西亚、印度 | 英文主版重点 QA 与首轮预约市场 |
| `ja` | 日本 | 日文页面优先覆盖 |
| `ko` | 韩国 | 韩文页面优先覆盖 |

### 首批开放地区的收缩顺序

如果后台不能直接做全球预约页，则按以下顺序收缩：

1. 台湾、香港、澳门
2. 日本、韩国
3. 美国、加拿大、英国、澳大利亚、新西兰
4. 新加坡、菲律宾、马来西亚、印度

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
2. 使用 `夜幕幸存者 / Nightfall Survivors` 等正式命名冻结主视觉
3. 提审 Android 正式包
4. 提审 Windows 纯净包

## 外部后台说明

- 预约/关注页阶段只填商店资料，不上传正式下载包
- 国区与 International 按不同语言口径填写，不混用标题
- Windows 包建议保持解压即玩，不做安装器优先
- Android 发布包必须使用正式签名，不要提交 debug 包
