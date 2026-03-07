# TapTap Windows 包整理指南

TapTap Windows 版本建议提交“纯净解压即玩包”。本仓库已提供整理脚本：

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Release/Prepare-TapTapWindowsPackage.ps1
```

## 默认输入与输出

- 输入目录：`Build/Windows`
- Stage 目录：`Release/TapTap/Windows/Stage`
- 输出目录：`Release/TapTap/Windows`
- 默认格式：`.zip`

## 脚本行为

- 自动识别主游戏 `.exe`
- 将最终入口统一整理为 `Nightfall Survivors.exe`
- 复制主程序与对应 `_Data` 目录
- 保留运行所需的 `UnityPlayer.dll`、`GameAssembly.dll`、`baselib.dll`
- 自动剔除 `*_BurstDebugInformation_DoNotShip*`
- 自动剔除 `*_BackUpThisFolder_ButDontShipItWithYourGame*`

## 常用示例

生成默认 zip：

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Release/Prepare-TapTapWindowsPackage.ps1
```

仅生成纯净 Stage 目录，不压缩：

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Release/Prepare-TapTapWindowsPackage.ps1 -SkipArchive
```

如果系统已安装 7-Zip，也可以直接生成 `.7z`：

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Release/Prepare-TapTapWindowsPackage.ps1 -ArchiveFormat 7z
```

## 后台填写建议

- 启动相对路径：`Nightfall Survivors.exe`
- 包体根目录：解压后应直接看到 `.exe` 与 `_Data`
- 不要上传带安装器外壳的二次封装包
