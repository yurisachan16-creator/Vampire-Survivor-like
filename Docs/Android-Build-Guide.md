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

## 五、真机验证（USB 连接）

### 1. 手机侧

- 打开开发者选项
- 开启 USB 调试
- 首次连接弹窗选择“允许此电脑调试”

### 2. 电脑侧

- 建议通过 Unity Hub 安装 Android Build Support（SDK/NDK/OpenJDK）
- 部分机型需要安装对应品牌的 USB 驱动（设备管理器能识别手机为佳）

### 3. Unity 侧（运行到手机）

1. `File → Build Settings`
2. `Platform` 选择 `Android`，点击 `Switch Platform`（若已切换可跳过）
3. `Scenes In Build` 顺序确认：
   - `Assets/Scenes/GameStart.unity`（索引 0）
   - `Assets/Scenes/Game.unity`（索引 1）
4. `Run Device` 下拉选择你的手机
5. 点击 `Build And Run`，选择输出目录（建议 `Build/Android/Development/`）

> 如果底部提示 “Cannot build player while editor is importing assets or compiling scripts”，等待导入/编译结束后再构建。

---

## 六、真机验证清单（最小闭环）

- 首次启动可进入主界面并开始游戏
- 触控/按钮可完成全流程（无键盘）
- Android 返回键行为符合预期（关闭弹窗→返回上一级→暂停/退出）
- 前后台切换后可继续游戏（不黑屏/不丢输入）
- 本地存档在重启后可恢复（PlayerPrefs）

### 重点验证（本次触控改造）

- 游戏内左下出现虚拟摇杆，拖动即可移动，松手停止
- 游戏内右上出现暂停按钮，点击等价于返回键：打开/关闭设置面板
- 系统返回键（物理键/手势返回）行为与暂停按钮一致

---

## 七、常见问题排查

### 1. Run Device 下拉找不到手机

- 手机 USB 连接模式改为“文件传输（MTP）”
- 重新插拔数据线/更换 USB 口/更换数据线
- 重新允许 USB 调试授权
- 安装/更新手机品牌 USB 驱动

### 2. 真机黑屏/闪退/资源缺失

- 优先检查是否已经为 Android 构建了 AssetBundle：`Assets/StreamingAssets/AssetBundles/Android/`
- 使用 Logcat 抓日志定位（Android Studio Logcat 或 Unity 的 Android Logcat 包）
- 重点关注关键词：AssetBundle/Shader/ABI/IL2CPP/NullReferenceException

### 3. Build And Run 提示 Unable to reverse network traffic to device

- 这通常只影响 Unity 的“推送到设备/反向端口”步骤，不影响 `Build` 生成 APK
- 建议先用 `Build` 生成 APK，再用 ADB 安装到手机验证：
  - `adb devices`（确认设备为 `device` 状态，非 `unauthorized`）
  - `adb install -r <apk路径>`

### 4. Release 构建弹窗：Could not resolve all files for configuration ':launcher:releaseCompileClasspath'

- 该问题常见于 `:launcher:lintVitalAnalyzeRelease` 触发时对 AAR 做 lint 解包失败
- 处理方式：
  - 先用 `Development Build` 验证功能闭环（不走 release lintVital）
  - 本项目已提供 `Assets/Plugins/Android/launcherTemplate.gradle` 并禁用 Release Lint（checkReleaseBuilds=false），用于稳定 Release 构建
  - 如仍偶发：运行清缓存脚本 `Tools/Android/Clean-UnityAndroidGradleCache.ps1` 后重建

### 3. 调试 HUD 与日志导出（Development Build）

- 设置面板里可开启“调试HUD”（仅真机调试包显示）
- HUD 手势：
  - 顶部区域三指同时点按：显示/隐藏 HUD
  - 顶部区域四指同时点按：导出最近日志到 `Application.persistentDataPath`
- 导出的文件位于：`Android/data/<包名>/files/`（可用 `adb pull` 或文件管理器取出）
