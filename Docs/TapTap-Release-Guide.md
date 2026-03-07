# TapTap 上架执行指南

本指南整理了当前仓库里已经落地的 TapTap 发布入口、文档和脚本，目标是先完成预约/关注页，再补 Android 与 Windows 包体。

## 当前仓库默认值

- Unity：`2022.3.62f2`
- 工作室名称：`Yurisa Project`
- Unity `productName`：`Nightfall Survivors`
- 商店中文展示名：`夜幕幸存者`
- 发布基线版本：`1.0.0`
- Android `applicationId`：`com.yurisa.nightfallsurvivors`
- Standalone `applicationId`：`com.yurisa.nightfallsurvivors`
- Android `versionCode`：`100`
- Android Target Architectures：`ARM64`
- Windows 启动相对路径：`Nightfall Survivors.exe`
- Android keystore alias：`nightfallsurvivors-release`
- Android keystore 默认路径：`%USERPROFILE%\.keystores\NightfallSurvivors\nightfallsurvivors-release.keystore`
- 正式中文名：`夜幕幸存者`
- 繁體中文名：`夜幕倖存者`
- 英文名：`Nightfall Survivors`
- 日文名：`宵闇サバイバーズ`
- 韩文名：`암야의 생존자들`
- 联系邮箱：`yurisachan16@gmail.com`

> 对外公开商店页、素材和隐私政策时，统一使用 `夜幕幸存者 / Nightfall Survivors`，不再沿用历史工程代号。

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

## Android 签名准备

推荐使用仓库外固定 keystore，不把密钥文件和密码提交到版本库。

1. 生成 keystore：

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Release/New-AndroidReleaseKeystore.ps1
```

2. 默认输出位置：
   `C:\Users\<你的用户名>\.keystores\NightfallSurvivors\nightfallsurvivors-release.keystore`
3. Unity `Player Settings > Publishing Settings` 中保持：
   - `Custom Keystore` 已启用
   - `Keystore` 路径为上述固定路径
   - `Alias` 为 `nightfallsurvivors-release`
4. 密码只在本机 Unity 或本机环境变量中填写，不写入仓库。
5. 出 Android 正式包前执行：

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Release/Test-TapTapReleaseConfig.ps1 -RequireKeystore
```

## 首次版本策略

- 对外版本号：`1.0.0`
- Android `versionCode`：`100`
- 后续热修复建议按 `1.0.1 / 101`、`1.0.2 / 102` 递增
- 只要重新提审 Android，`versionCode` 必须继续上升，不能回退

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
- `Docs/TapTap-Backend-Final-Entry.md`
- `Docs/TapTap-Trial-Notice.md`
- `Docs/TapTap-Store-Assets-Checklist.md`
- `Docs/TapTap-Privacy-Policy-Template.md`
- `Docs/TapTap-Submission-Checklist.md`
- `Docs/TapTap-Windows-Package-Guide.md`

## 命令行脚本

- `Tools/Release/Test-TapTapReleaseConfig.ps1`
  - 校验 `ProjectSettings.asset` 中的公司名、产品名、版本号、包名、Target SDK、ARM64 与签名路径配置
  - `-RequireKeystore` 会额外检查 keystore 文件是否真实存在
- `Tools/Release/New-AndroidReleaseKeystore.ps1`
  - 生成项目专用 Android 发布 keystore
  - 默认写到 `%USERPROFILE%\.keystores\NightfallSurvivors\`
- `Tools/Release/Prepare-TapTapWindowsPackage.ps1`
  - 从 `Build/Windows` 提取纯净文件
  - 会把最终入口统一整理成 `Nightfall Survivors.exe`
  - 默认输出 `.zip`
  - 会自动剔除 `DoNotShip` 调试目录
- `Tools/Release/Test-TapTapStoreReadiness.ps1`
  - 汇总检查文档、素材、Windows 包、Android APK 与隐私政策链接占位
  - 可选 `-RequireKeystore`，用于确认 Android 已切换到正式签名准备状态

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

## 当前缺口如何落地

仓库内已补以下准备产物：

- `Release/TapTap/Assets/`：最终商店素材产出目录
- `Release/TapTap/Assets/asset-manifest.json`：素材缺口清单
- `Release/TapTap/Assets/Policies/privacy-policy.md`：公开隐私政策源文件
- `Docs/TapTap-Backend-Final-Entry.md`：后台逐字段最终录入稿
- `Docs/TapTap-Trial-Notice.md`：试玩说明定稿

仍需你在仓库外或手工补齐的内容：

- 正式安卓 keystore 文件与密码的本机保存方式
- 公网可访问的隐私政策 URL
- 最终商店图标、封面、截图、视频封面
- 真机回归记录
