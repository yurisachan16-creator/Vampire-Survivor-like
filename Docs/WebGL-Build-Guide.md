# WebGL 构建与 itch.io 发布指南

## 一、前置准备

### 1. 安装 WebGL 构建模块

在 Unity Hub 中为 Unity 2022.3.62f2 安装 WebGL Build Support：
1. 打开 Unity Hub → Installs
2. 找到 2022.3.62f2c1 → 点击齿轮 → Add Modules
3. 勾选 **WebGL Build Support** → Install

### 2. 项目兼容性检查

以下功能在 WebGL 中需要注意：

| 功能 | 兼容性 | 本项目情况 |
|------|--------|-----------|
| PlayerPrefs | ✅ 支持 (IndexedDB) | 存档系统可正常使用 |
| AudioKit | ✅ 需用户交互后播放 | 首次点击后正常 |
| Time.timeScale | ✅ 支持 | 暂停功能正常 |
| 多线程 | ❌ 受限 | 本项目未使用 |
| 文件系统 | ❌ 不支持 | 本项目未使用 |

---

## 二、Unity 项目配置

### 1. 切换构建平台

```
File → Build Settings → WebGL → Switch Platform
```

### 2. Player Settings 配置 (Edit → Project Settings → Player)

#### Resolution and Presentation
- **Default Canvas Width**: `960`
- **Default Canvas Height**: `600`（或 `1280 x 720` 等 16:9 分辨率）
- **Run In Background**: ✅ 勾选

#### Publishing Settings
- **Compression Format**: `Gzip`（itch.io 支持）
- **Decompression Fallback**: ✅ 勾选（兼容不支持原生解压的浏览器）
- **Data Caching**: ✅ 勾选（提升二次加载速度）

#### Other Settings
- **Color Space**: `Gamma`（WebGL 兼容性更好，或保持 Linear 但需 WebGL 2.0）
- **Auto Graphics API**: 取消勾选，仅保留 **WebGL 2.0**
- **Strip Engine Code**: ✅ 勾选（减小包体）
- **Managed Stripping Level**: `Medium` 或 `High`

> 如果出现“只有背景/音乐，但开始界面(UI)不显示”，优先把 **Managed Stripping Level** 调低（建议 `Low`），
> 因为 UIKit/反射/泛型在 IL2CPP + 裁剪较高时可能会被裁掉类型。
> 也建议把 **WebGL Memory Size / Initial Memory** 提高到至少 `256MB`（本项目默认 1080p UI 与较多资源，32MB 很容易出问题）。

### 3. 确保场景已添加

Build Settings 中确认包含：
1. `Assets/Scenes/GameStart.unity` (index 0)
2. `Assets/Scenes/Game.unity` (index 1)

### 4. （本项目关键）为 WebGL 打 AssetBundle

本项目的 UI Prefab / 音频等资源使用 QFramework ResKit 的 **AssetBundle** 管理（可在 `Assets/QFrameworkData/QAssets.cs` 看到已生成的 Bundle 名称）。
如果你只打了 Windows 的 AB 包，那么 WebGL 运行时会去请求 `StreamingAssets/AssetBundles/WebGL/...`，导致 UI Prefab 加载失败，从而出现：
**场景有画面/音乐，但开始界面(UI)不显示**。

检查方法（无需猜）：
- 打开 WebGL 构建目录，确认存在 `Build/WebGL/StreamingAssets/AssetBundles/WebGL/`（而不是只有 `Windows/`）
- 浏览器 DevTools → Network，看看是否有对 `StreamingAssets/AssetBundles/WebGL/...` 的请求 404

修复步骤：
1. Unity 菜单打开 QFramework 的 ResKit 设置（你截图里的页面）
2. **目标平台**选择 `WebGL`
3. 点击 **打 AB 包**（必要时先点“生成代码(资源名称常量)”）
4. 确认生成目录为：`Assets/StreamingAssets/AssetBundles/WebGL/`
5. 重新 Build WebGL（让这些 AB 文件被带进 `Build/WebGL/StreamingAssets/AssetBundles/WebGL/`）

> 备注：你目前本地构建输出里只有 `Build/WebGL/StreamingAssets/AssetBundles/Windows/`，这就是 UI 不显示的最强嫌疑点。

---

## 三、构建 WebGL

### 1. 构建步骤

```
File → Build Settings → WebGL → Build
```

选择输出目录：`Build/WebGL/`

### 2. 构建产物

构建完成后，`Build/WebGL/` 目录结构：
```
Build/WebGL/
├── index.html           # 入口页面
├── Build/
│   ├── WebGL.data.gz    # 游戏资源（压缩）
│   ├── WebGL.framework.js.gz
│   ├── WebGL.loader.js
│   └── WebGL.wasm.gz    # WebAssembly 代码
└── TemplateData/        # Unity 默认模板资源
```

---

## 四、本地测试

WebGL 构建需要通过 HTTP 服务器访问（不能直接打开 index.html）。

### 方法 1：Python 简易服务器

```powershell
cd "d:\unity\Vampire Survivor-like\Build\WebGL"
python -m http.server 8080
```

然后访问：http://localhost:8080

### 方法 2：VS Code Live Server 扩展

1. 安装 Live Server 扩展
2. 右键 `index.html` → Open with Live Server

---

## 五、itch.io 发布

### 1. 注册与创建项目

1. 访问 https://itch.io 并注册账号
2. Dashboard → Create new project

### 2. 项目配置

| 字段 | 推荐值 |
|------|--------|
| Title | Nightfall Survivors |
| Kind of project | HTML |
| Pricing | Free / Name your own price |
| Uploads | 见下方 |

### 3. 上传构建

1. **打包 WebGL 构建**：将 `Build/WebGL/` 文件夹压缩为 `.zip`
2. **上传设置**：
   - Upload → 选择 `.zip` 文件
   - 勾选 **This file will be played in the browser**
3. **Embed options**：
   - 设置嵌入尺寸（如 `960 x 600` 或 `1280 x 720`）
   - 勾选 **Enable scrollbars** 如需要
   - 勾选 **Fullscreen button** 添加全屏按钮

### 4. 发布

点击 **Save & view page** 预览，确认无误后点击 **Publish**

---

## 六、常见问题

### Q1: 游戏黑屏/无法加载

- 确保使用 **Gzip 压缩** 并勾选 **Decompression Fallback**
- 检查浏览器控制台（F12）是否有 CORS 或 MIME type 错误
- itch.io 会自动处理 `.gz` 文件的 Content-Encoding

定位技巧：打开 DevTools → Console/Network，刷新页面后重点看是否有 `TypeLoadException` / `MissingMethodException` / `Out of memory`。

### Q2: 音频不播放

WebGL 要求用户交互后才能播放音频。解决方案：
- 在开始界面添加"点击开始"按钮（本项目 GameStart 场景已有）

### Q3: 游戏卡顿

- 降低同屏敌人/特效数量
- 使用 `IL2CPP` 代码优化（默认已开启）
- 考虑使用 `Development Build` 分析性能

### Q4: 存档丢失

WebGL 使用 IndexedDB 存储 PlayerPrefs：
- 清除浏览器数据会丢失存档
- 隐私/无痕模式可能不持久化

### Q5: 内存不足 (Out of Memory)

- 减小纹理分辨率
- 启用 **Memory Size** 限制（Player Settings → WebGL → Memory Size）
- 默认 256MB，可尝试增至 512MB

---

## 七、优化建议

### 减小包体大小

1. **压缩纹理**：使用 ASTC/ETC2 格式
2. **音频压缩**：使用 Vorbis 格式，降低采样率
3. **剥离未使用代码**：Managed Stripping Level 设为 High
4. **移除未使用场景**：确保 Build Settings 只包含必要场景

### 加载体验

1. 自定义加载页面模板：`Assets/WebGLTemplates/`
2. 添加加载进度条和提示文字

---

## 附录：快速构建脚本

创建 `Assets/Editor/WebGLBuilder.cs`：

```csharp
using UnityEditor;
using UnityEngine;

public class WebGLBuilder
{
    [MenuItem("Build/WebGL Build")]
    public static void Build()
    {
        var scenes = new[] {
            "Assets/Scenes/GameStart.unity",
            "Assets/Scenes/Game.unity"
        };
        
        BuildPipeline.BuildPlayer(scenes, "Build/WebGL", 
            BuildTarget.WebGL, BuildOptions.None);
        
        Debug.Log("WebGL Build Complete!");
    }
}
```

使用：Unity 菜单 → Build → WebGL Build
