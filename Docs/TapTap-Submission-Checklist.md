# TapTap 提审前检查清单

## 商店页

- [ ] 正式中文名 `夜幕幸存者` 与英文名 `Nightfall Survivors` 已冻结
- [ ] 所有公开素材已替换工程代号
- [ ] 简中页面可完整预览
- [ ] 英文、日文、韩文、繁中字段已补齐
- [ ] 预约/关注按钮流程可用
- [ ] 平台说明写明 Android 首发、Windows 随后开放
- [ ] 隐私说明与当前代码行为一致
- [ ] 国区与 International 的标题口径没有混用
- [ ] 国区页面主语言为简中
- [ ] International 页面主语言为英文，已补繁中、日文、韩文
- [ ] `language_region_priority` 对应的重点国家/地区已确认

## Android 包

- [ ] `applicationId` 不再是 `com.DefaultCompany.*`
- [ ] `bundleVersion` 已更新到本次提审版本
- [ ] `versionCode` 高于上一次提交
- [ ] `Target SDK` 已固定，不是 Auto
- [ ] `IL2CPP + ARM64` 配置已确认
- [ ] 已重新构建 Android AssetBundle
- [ ] 使用正式 keystore 签名
- [ ] 安装包可安装、可启动、可进入战斗
- [ ] 触屏完整流程可玩
- [ ] 存档重启后可恢复

## Windows 包

- [ ] 解压目录根部可直接看到 `Vampire Survivor-like.exe`
- [ ] `_Data` 目录与主程序同级
- [ ] 不包含 `DoNotShip` 调试目录
- [ ] 解压即玩，无额外安装步骤
- [ ] 首次启动正常
- [ ] 进入战斗、退出重进正常
- [ ] 压缩包格式为 `.zip` 或 `.7z`

## 审核材料

- [ ] 图标、封面、截图、简介一致指向同一正式名称
- [ ] 关键词和分类与玩法一致
- [ ] 当前阶段说明为“预约中”或对应测试状态
- [ ] 联系方式已填写为 `yurisachan16@gmail.com`
- [ ] 宣传视频、截图和文案没有占位内容
- [ ] 中国区页面可直接用简中资料录入
- [ ] International 页面可直接用英文主文案及繁中、日文、韩文补充录入
- [ ] 若后台需要挑选重点国家/地区，可直接使用既定语言矩阵

## 本地命令

发布前建议至少执行：

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Release/Test-TapTapReleaseConfig.ps1
```

Windows 包整理：

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Release/Prepare-TapTapWindowsPackage.ps1
```
