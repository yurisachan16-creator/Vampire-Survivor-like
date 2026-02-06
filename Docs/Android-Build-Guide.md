# Android 构建指南（TapTap）

## 一、前置准备

### 1. 安装 Android 构建模块

在 Unity Hub 中为 Unity 2022.3.62f2 安装 Android Build Support（包含 SDK & NDK Tools、OpenJDK）。

### 2. 项目基本信息

在 `Edit → Project Settings → Player` 中确认以下信息：
- **Company Name / Product Name**：用于生成包名与应用显示名
- **Package Name（Application Identifier）**：Android 必须是唯一包名（当前工程已写入一个占位值：`com.DefaultCompany.VampireSurvivorLike`，发布前请替换）
- **Version / Bundle Version Code**：对外版本号 + 递增的内部版本号

### 3. 推荐的 Android Player Settings（无支付/单机版）

#### Other Settings
- **Scripting Backend**：IL2CPP
- **Target Architectures**：ARM64（可按渠道要求补 ARMv7）
- **Minimum API Level**：Android 5.1（API 22）或更高（按你的目标用户调整）
- **Target API Level**：建议在发布分支固定到平台要求的版本（避免“自动”导致可重复性不足）

#### Resolution and Presentation
- **Default Orientation**：按游戏设计选择（通常横屏）
- **Render Outside Safe Area**：关闭（配合 Safe Area 适配）

#### Publishing Settings
- **Custom Keystore**：发布包必须使用固定 keystore（测试/发布分离；不要提交到仓库）

---

## 二、切换构建平台

```
File → Build Settings → Android → Switch Platform
```

确认 Scenes in Build 包含：
1. `Assets/Scenes/GameStart.unity`
2. `Assets/Scenes/Game.unity`

---

## 三、（本项目关键）为 Android 打 AssetBundle

本项目的 UI Prefab / 音频等资源使用 QFramework ResKit 的 **AssetBundle** 管理（参见 ResKit 的 AB 构建脚本：[BuildScript](file:///d:/unity/Vampire%20Survivor-like/Assets/QFramework/Toolkits/ResKit/Editor/BuildScript.cs#L31-L90)）。

如果只打了 Windows/WebGL 的 AB 包，Android 运行时会尝试加载 `StreamingAssets/AssetBundles/Android/...`，从而出现 UI/资源加载失败。

检查方法：
- 确认存在目录：`Assets/StreamingAssets/AssetBundles/Android/`
- 确认其中包含 `asset_bundle_config.bin` 等配置文件

构建步骤（按 ResKit 面板操作）：
1. 目标平台选择 `Android`
2. 点击“打 AB 包”
3. 确认输出目录为：`Assets/StreamingAssets/AssetBundles/Android/`

---

## 四、构建 Android 包

```
File → Build Settings → Android → Build
```

建议输出目录：
- `Build/Android/`（Debug/Development）\n+- `Release/Android/`（Release）

---

## 五、真机验证清单（最小闭环）

- 首次启动可进入主界面并开始游戏
- 触控/按钮可完成全流程（无键盘）
- Android 返回键行为符合预期（关闭弹窗→返回上一级→暂停/退出）
- 前后台切换后可继续游戏（不黑屏/不丢输入）
- 本地存档在重启后可恢复（PlayerPrefs）

