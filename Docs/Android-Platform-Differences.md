# 平台差异清单（Web / Windows / Android）

## 1. 输入与交互

### 现状
- 多处逻辑直接依赖键盘按键（例如 ESC、调试键 L）。
  - 示例：ESC 退出/返回（[GameStartController](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/Game/GameStartController.cs#L31-L37)）
  - 示例：调试显示存档键 L（[SaveSystem](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/System/Save/SaveSystem.cs#L56-L69)）

### Android 目标行为
- 全流程可通过触控完成（虚拟摇杆/按钮）。
- Android 返回键等价于“Back/ESC”，按 UI 层级处理。

---

## 2. 分辨率与 UI

### 现状
- PC/桌面可通过分辨率/全屏逻辑控制窗口；移动端主要依赖 UI 自适配与 Safe Area。

### Android 目标行为
- Safe Area 适配刘海/挖孔屏。
- 适配极端比例（20:9、21:9），关键 UI 不被裁切。
- 明确横竖屏策略与禁止自动旋转（如不需要）。

---

## 3. 资源加载与 AssetBundle

### 现状
- 使用 QFramework ResKit 的 AssetBundle 管线（[BuildScript](file:///d:/unity/Vampire%20Survivor-like/Assets/QFramework/Toolkits/ResKit/Editor/BuildScript.cs#L31-L90)）。
- 仓库内可见 WebGL 平台 AB 产物，但 Android 平台 AB 需要单独构建。
### Android 风险点
- 未携带 `StreamingAssets/AssetBundles/Android/` 会导致运行时资源加载失败。
- Android StreamingAssets 读取路径与打包方式不同（ResKit 内部有 APK-as-zip 读取逻辑：[ZipFileHelper](file:///d:/unity/Vampire%20Survivor-like/Assets/QFramework/Toolkits/ResKit/Scripts/Architecture/Utility/ZipFileHelper.cs#L27-L136)）。
---

## 4. 本地存档（无云/无账号）

### 现状
- 使用 PlayerPrefs 保存关键数据（[SaveSystem](file:///d:/unity/Vampire%20Survivor-like/Assets/Scripts/System/Save/SaveSystem.cs#L20-L54)）。
### 平台差异说明
- WebGL：存档落在浏览器 IndexedDB/站点数据。
- Windows：存档在本机用户目录对应位置。
- Android：存档在应用私有目录；卸载会丢失。
---
- 商店说明中明确“多端存档不互通”。
- 可选提供“导出/导入存档”能力，方便玩家手动迁移。

---

## 5. 构建与版本

### Android
- 包名必须唯一（已设置占位包名，发布前替换）。
- 版本号策略：对外 versionName + 递增 versionCode。
- 签名：发布包必须固定 keystore（测试/发布分离）。
