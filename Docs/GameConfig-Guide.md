# 游戏数值配置系统使用指南

## 概述

本系统允许你通过 Excel/CSV 文件配置游戏数值参数，包括：
- **敌人波次配置**：敌人生成规则、属性倍率、掉落概率等
- **技能属性配置**：武器伤害、攻击间隔、弹射数量等

实现数值平衡的外部化管理，无需修改代码即可调整游戏难度和手感。

## 相关文档

- 波次系统调用链与切波逻辑说明：`Assets/Docs/WaveSystem.md`
- 多语言（字符串表/导入导出工作流）：`Docs/I18N.md`
- v0.10 版本线回溯（本分支整理）：`Docs/Changelog-v0.10.md`

---

# 一、敌人波次配置

## 文件位置

- **配置文件**: `Assets/StreamingAssets/Config/EnemyWaveConfig.csv`
- **预制体映射**: `Assets/Art/Config/EnemyPrefabMapping.asset`（需要在 Unity 中创建）

## CSV 配置格式

### 基础字段

| 列名 | 说明 | 示例值 |
|------|------|--------|
| GroupName | 波次组名称 | 第一波幽灵 |
| GroupDescription | 波次组描述 | 第一波幽灵敌人 |
| WaveName | 具体波次名称 | 幽灵 |
| Active | 是否启用 | TRUE/FALSE |
| EnemyPrefabName | 敌人预制体名称（与 EnemyPrefabMapping 中的 Name 对应） | EnemyA |
| GenerateDuration | 生成间隔（秒） | 0.8 |
| KeepSeconds | 持续生成时间（秒） | 15 |
| HPScale | 血量倍率 | 1.0 |
| SpeedScale | 速度倍率 | 1.0 |
| DamageScale | 伤害倍率 | 1.0 |

### 扩展字段（新增）

| 列名 | 说明 | 默认值 | 示例值 |
|------|------|--------|--------|
| BaseSpeed | 敌人基础移动速度 | 1.0 | 1.5 |
| IsTreasureChest | 是否掉落宝箱（用于 Boss） | FALSE | TRUE |
| ExpDropRate | 经验掉落概率 (0-1) | 0.6 | 0.8 |
| CoinDropRate | 金币掉落概率 (0-1) | 0.2 | 0.15 |
| HpDropRate | 血瓶掉落概率 (0-1) | 0.15 | 0.1 |
| BombDropRate | 炸弹掉落概率 (0-1) | 0.05 | 0.02 |

> **注意**: 掉落概率之和应 ≤ 1.0，剩余概率为不掉落任何物品

## 使用步骤

### 1. 创建敌人预制体映射

1. 在 Unity 中，右键 `Assets/Art/Config` 文件夹
2. 选择 **Create → VampireSurvivorLike → EnemyPrefabMapping**
3. 命名为 `EnemyPrefabMapping`
4. 选中新创建的文件，在 Inspector 中右键选择 **Auto Load Enemy Prefabs** 自动加载所有敌人预制体
5. 或者手动添加敌人条目：
   - 点击 Enemies 列表的 `+` 按钮
   - 填写 Name（与 CSV 中的 EnemyPrefabName 对应）
   - 拖入对应的敌人预制体

### 2. 配置 EnemyGenerator

1. 在场景中找到 `EnemyGenerator` 对象
2. 在 Inspector 中：
   - 勾选 **Use CSV Config** 启用 CSV 配置
   - 将创建的 `EnemyPrefabMapping` 拖入 **Prefab Mapping** 字段

### 3. 编辑 CSV 配置

1. 使用 Excel 或 WPS 打开 `Assets/StreamingAssets/Config/EnemyWaveConfig.csv`
2. 根据需要修改参数
3. 保存时选择 **CSV UTF-8（逗号分隔）** 格式

## 敌人属性配置示例

### Boss 配置（掉落宝箱）
```csv
Boss_Slime,,大史莱姆,TRUE,EnemyMiniBoss,10,30,8.0,0.6,3.0,0.8,TRUE,0,0,0,0
```
- `IsTreasureChest=TRUE`: 击杀后掉落宝箱
- 掉落概率全为 0：Boss 只掉宝箱，不掉普通物品

### 高经验敌人
```csv
Wave_1,第一波幽灵,幽灵,TRUE,EnemyA,0.8,15,1.0,1.0,1.0,1.0,FALSE,0.9,0.05,0.03,0.02
```
- `ExpDropRate=0.9`: 90% 概率掉落经验

### 快速敌人
```csv
Fast_Enemy,,快速幽灵,TRUE,EnemyA,0.5,20,0.8,1.5,0.8,1.5,FALSE,0.6,0.2,0.15,0.05
```
- `BaseSpeed=1.5`: 基础速度 1.5 倍
- `SpeedScale=1.5`: 速度倍率 1.5 倍（最终速度 = BaseSpeed × SpeedScale）

## 数值平衡建议

### 早期波次（0-3分钟）
- GenerateDuration: 0.8-1.2 秒
- KeepSeconds: 10-20 秒
- HPScale: 1.0
- SpeedScale: 1.0
- DamageScale: 1.0

### 中期波次（3-7分钟）
- GenerateDuration: 0.4-0.8 秒
- KeepSeconds: 25-40 秒
- HPScale: 1.5-2.5
- SpeedScale: 1.0-1.2
- DamageScale: 1.2-1.5

### 后期波次（7-12分钟）
- GenerateDuration: 0.2-0.5 秒
- KeepSeconds: 40-60 秒
- HPScale: 2.0-4.0
- SpeedScale: 1.2-1.5
- DamageScale: 1.5-2.0

### Boss 波次
- GenerateDuration: 1.5-3.0 秒（生成频率低）
- KeepSeconds: 30-60 秒
- HPScale: 5.0-20.0
- SpeedScale: 0.3-0.7（移动较慢但更有威胁）
- DamageScale: 2.0-6.0

## 切换配置模式

- **Use CSV Config = true**: 从 CSV 文件加载波次配置
- **Use CSV Config = false**: 使用原有的 LevelConfig ScriptableObject 配置

## 注意事项

1. CSV 文件必须使用 **UTF-8** 编码保存
2. EnemyPrefabName 必须与 EnemyPrefabMapping 中的 Name **完全匹配**
3. WebGL 平台完全支持异步加载
4. 修改 CSV 后无需重新打包，直接替换文件即可
5. Active 列设为 FALSE 可临时禁用某个波次

## 文件结构

```
Assets/
├── StreamingAssets/
│   └── Config/
│       ├── EnemyWaveConfig.csv      # 敌人波次配置表
│       └── AbilityConfig.csv        # 技能属性配置表
├── Scripts/
│   └── Config/
│       ├── EnemyWaveConfigLoader.cs # 敌人 CSV 加载器
│       ├── AbilityConfigLoader.cs   # 技能 CSV 加载器
│       ├── EnemyPrefabMapping.cs    # 预制体映射
│       └── LevelConfig.cs           # 原有配置（已扩展）
└── Art/
    └── Config/
        └── EnemyPrefabMapping.asset # 预制体映射实例（需创建）
```

---

# 二、技能属性配置

## 文件位置

- **配置文件**: `Assets/StreamingAssets/Config/AbilityConfig.csv`

## CSV 配置格式

| 列名 | 说明 | 示例值 |
|------|------|--------|
| AbilityKey | 技能唯一标识（与代码中的 Key 对应） | simple_sword |
| Damage | 基础伤害值 | 3 |
| Duration | 攻击/持续时间（秒） | 0.2 |
| Count | 弹射/分裂数量 | 1 |
| Range | 攻击范围 | 2.0 |
| Speed | 弹射速度 | 10.0 |
| AttackCount | 攻击次数（穿透数） | 1 |

## 技能 Key 与代码对应关系

| AbilityKey | 对应脚本 | 说明 |
|------------|----------|------|
| simple_sword | SimpleSword.cs | 简易剑 |
| simple_knife | SimpleKnife.cs | 简易飞刀 |
| rotate_sword | RotateSword.cs | 旋转剑 |
| basket_ball | BasketBall.cs | 篮球 |
| bomb | Bomb.cs | 炸弹 |

## 使用方法

### 1. 在代码中应用配置

技能在初始化时从 `Global.cs` 获取配置：

```csharp
// 在 Global.ResetData() 中自动调用
public static void ApplyAbilityConfig()
{
    var swordConfig = AbilityConfigLoader.Instance.GetConfig("simple_sword");
    SimpleSwordDamage.Value = swordConfig.Damage;
    SimpleSwordDuration.Value = swordConfig.Duration;
    SimpleSwordCount.Value = swordConfig.Count;
    // ...
}
```

### 2. 添加新技能配置

1. 在 `AbilityConfig.csv` 中添加新行
2. 在 `Global.cs` 中添加对应的 `BindableProperty`
3. 在 `ApplyAbilityConfig()` 中读取并设置值
4. 在技能脚本中使用 `Global.XXX.Value` 获取配置值

## 配置示例

```csv
AbilityKey,Damage,Duration,Count,Range,Speed,AttackCount
simple_sword,3,0.2,1,2.0,0,1
simple_knife,2,0.5,3,0,15.0,1
rotate_sword,2,0,3,1.5,180,99
basket_ball,4,1.0,1,0,8.0,5
bomb,10,3.0,1,3.0,0,1
```

---

# 三、常见问题

## Q: 修改 CSV 后需要重新打包吗？
A: 不需要。CSV 文件位于 StreamingAssets，运行时动态加载。

## Q: Excel 保存后中文变成乱码？
A: 请使用"另存为" → 选择"CSV UTF-8（逗号分隔）"格式。

## Q: 新增的敌人预制体如何添加？
A: 在 Unity 中选中 `EnemyPrefabMapping.asset`，右键选择 **Auto Load Enemy Prefabs** 自动加载。

## Q: WebGL 平台能否正常加载？
A: 完全支持。系统使用 UnityWebRequest 异步加载，兼容所有平台。

## Q: 如何临时禁用某个波次？
A: 将 `Active` 列设为 `FALSE` 即可。
