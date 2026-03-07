# TapTap 商店素材清单

## 必做素材

- 游戏图标 1 套
- 横版主视觉 1 套
- 竖版封面 1 套
- 实机截图至少 6 张
- 平台说明图 1 张
- 宣传视频 1 条（推荐，15 到 45 秒）

## 实机截图脚本

至少准备以下 6 张：

1. 开始界面
2. 战斗中高密度清怪画面
3. 升级三选一界面
4. 宝箱或武器进化画面
5. 永久升级或成就界面
6. 设置或触屏操作界面

## 素材原则

- 所有截图必须来自真实游戏画面
- 不使用历史工程代号作为公开标题
- 同一批素材中的游戏名称、Logo、色板与口号必须一致
- Android 与 Windows 素材文案保持统一，只在平台说明上区分
- 有触屏截图时，保留真实移动端操作 UI

## 出图前检查

- UI 文本无占位词
- 语言切换后的字体正常显示
- 无开发按钮、调试 HUD、报错日志、Unity 默认图标
- 画面比例符合 TapTap 后台要求
- 截图里出现的版本说明与商店文案一致

## 文件管理建议

建议目录：

- `Release/TapTap/Assets/Icon/`
- `Release/TapTap/Assets/Hero/`
- `Release/TapTap/Assets/Capsule/`
- `Release/TapTap/Assets/Screenshots/`
- `Release/TapTap/Assets/Video/`
- `Release/TapTap/Assets/Policies/`

建议命名：

- `icon-main.png`
- `hero-horizontal.png`
- `cover-vertical.png`
- `screenshot-01-start.png`
- `screenshot-02-battle.png`
- `screenshot-03-upgrade.png`
- `screenshot-04-evolution.png`
- `screenshot-05-meta.png`
- `screenshot-06-mobile-ui.png`
- `privacy-policy-url.txt`

## 多语言素材处理

- 简中为母版
- 英文、日文、韩文、繁中只替换必要文字层
- 画面主体、角色、背景与构图不变

## 多语言与地区一致性要求

- 中文主视觉标题使用 `夜幕幸存者`
- 英文主视觉标题使用 `Nightfall Survivors`
- 繁中版封面使用 `夜幕倖存者`
- 日文版封面使用 `宵闇サバイバーズ`
- 韩文版封面使用 `암야의 생존자들`
- 中国区素材优先简中
- International 素材至少准备英文主版，繁中、日文、韩文做文字替换版本
- 所有截图说明、宣传图、视频封面必须与语言地区矩阵一致，不混用中文标题与英文副标题

## 地区投放与 QA 优先顺序

- 国区：中国大陆，使用简中主版
- International 第一优先：台湾、香港、澳门
- International 第二优先：日本、韩国
- International 第三优先：美国、加拿大、英国、澳大利亚、新西兰
- International 第四优先：新加坡、菲律宾、马来西亚、印度

## 当前缺失状态

当前仓库已经有素材目录规范，但还没有最终可提交的图标、封面、截图和视频封面文件。提交前建议跑一次：

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Release/Test-TapTapStoreReadiness.ps1
```
