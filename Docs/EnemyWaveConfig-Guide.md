# 游戏数值配置系统使用指南

## 概述

本系统允许你通过 Excel/CSV 文件配置游戏数值参数，包括：
- **敌人时间轴配置**：基于时间窗口的多频道刷怪规则、属性倍率、掉落概率等
- **技能属性配置**：武器伤害、攻击间隔、弹射数量等

实现数值平衡的外部化管理，无需修改代码即可调整游戏难度和手感。

---

# 一、敌人时间轴配置（v2.0 — 模仿吸血鬼幸存者）

## 系统概念

采用**时间轴驱动**的刷怪系统（类似 Vampire Survivors）：

- 每种敌人配置为一个**频道（Channel）**，拥有独立的时间窗口 `[StartTimeSec, EndTimeSec]`
- 多个频道可**并行执行**，不同敌人同时出现在场景中
- **无需清波**——刷怪完全由游戏时钟驱动
- HP/速度/伤害/刷新频率随时间**自动递增**
- 30 分钟一局，到时出现死神（Reaper）

## 文件位置

- **配置文件**: `Assets/StreamingAssets/Config/EnemyWaveConfig.csv`
- **预制体映射**: `Assets/Art/Config/EnemyPrefabMapping.asset`
- **加载器**: `Assets/Scripts/Config/EnemyWaveConfigLoader.cs` (`SpawnChannelConfigLoader`)
- **控制器**: `Assets/Scripts/Game/EnemyGenerator.cs` (`TimelineController`)

## CSV 配置格式

### 所有字段

| 列名 | 说明 | 必填 | 默认值 | 示例值 |
|------|------|------|--------|--------|
| ChannelName | 频道名称（UI 显示用） | 是 | — | 幽灵_背景 |
| Active | 是否启用 | 是 | — | TRUE |
| EnemyPrefabName | 敌人预制体名称（与 EnemyPrefabMapping 对应） | 是 | — | Enemy_Ghost |
| Phase | 阶段标记 | 否 | small | small / boss |
| StartTimeSec | 频道激活时间（游戏秒） | 是 | — | 0 |
| EndTimeSec | 频道结束时间（-1 = 永不结束） | 是 | — | 1800 |
| SpawnIntervalSec | 生成间隔（秒），受难度缩放 | 否 | 1.0 | 0.8 |
| SpawnCount | 总生成数量限制（0 = 无限） | 否 | 0 | 1 |
| HPScale | 血量基础倍率 | 否 | 1.0 | 5.0 |
| SpeedScale | 速度基础倍率 | 否 | 1.0 | 0.6 |
| DamageScale | 伤害基础倍率 | 否 | 1.0 | 3.0 |
| BaseSpeed | 敌人基础移动速度 | 否 | 2.0 | 1.5 |
| IsTreasureChest | 是否掉落宝箱（用于 Boss） | 否 | FALSE | TRUE |
| ExpDropRate | 经验掉落概率 (0-1) | 否 | 0.3 | 0.8 |
| CoinDropRate | 金币掉落概率 (0-1) | 否 | 0.3 | 0.15 |
| HpDropRate | 血瓶掉落概率 (0-1) | 否 | 0.1 | 0.05 |
| BombDropRate | 炸弹掉落概率 (0-1) | 否 | 0.05 | 0.02 |

> **注意**: 掉落概率之和应 ≤ 1.0，剩余概率为不掉落任何物品

### 时间窗口说明

- `StartTimeSec` 和 `EndTimeSec` 定义频道的活跃窗口
- 当游戏时间 ≥ `StartTimeSec` 且 < `EndTimeSec` 时，该频道会按间隔生成敌人
- `EndTimeSec = -1` 表示频道一直持续到游戏结束
- 多个频道的时间窗口可以重叠，实现同时刷出不同类型敌人

### SpawnCount 说明

- `SpawnCount = 0`：无限生成（在时间窗口内持续刷怪）
- `SpawnCount = 1`：只生成 1 只（适合 Boss）
- `SpawnCount > 1`：生成指定数量后停止

## 难度自动递增

所有频道的属性都会随时间自动增长，**无需在 CSV 中手动配置递增**：

| 属性 | 公式 | 常量（Config.cs） |
|------|------|--------------------|
| HP | `HPScale × (1 + 0.25 × t)` | `HPGrowthPerMinute = 0.25f` |
| Speed | `SpeedScale × (1 + 0.05 × t)` | `SpeedGrowthPerMinute = 0.05f` |
| Damage | `DamageScale × (1 + 0.15 × t)` | `DamageGrowthPerMinute = 0.15f` |
| 刷新间隔 | `SpawnIntervalSec / (1 + 0.1 × t)` | `SpawnRateGrowthPerMinute = 0.1f` |

其中 `t` = 当前游戏时间（分钟）。

**示例**：一个 `HPScale=1.0` 的频道在第 10 分钟时，实际 HP 倍率 = `1.0 × (1 + 0.25 × 10) = 3.5`

## 配置示例

### 背景频道（全程刷怪）
```csv
幽灵_背景,TRUE,Enemy_Ghost,small,0,1800,1.2,0,1.0,1.0,1.0,2.0,FALSE,0.3,0.3,0.1,0.05
```
- `StartTimeSec=0, EndTimeSec=1800`：整场游戏都在刷
- `SpawnCount=0`：无限生成
- 属性倍率均为 1.0，随时间自动增长

### Boss 频道（定时出现）
```csv
幽灵Boss_5min,TRUE,Enemy_Ghost_Boss,boss,300,360,1,1,5.0,0.6,3.0,1.5,TRUE,0,0,0,0
```
- `StartTimeSec=300`：第 5 分钟出现
- `SpawnCount=1`：只生成 1 只
- `IsTreasureChest=TRUE`：击杀掉宝箱
- 掉落概率全 0：Boss 只掉宝箱

### 中期新敌人（限时窗口）
```csv
方块_中期,TRUE,Enemy_Block_Yellow,small,180,600,0.8,0,1.5,1.0,1.2,2.5,FALSE,0.3,0.3,0.1,0.05
```
- `StartTimeSec=180, EndTimeSec=600`：3-10 分钟出现
- 可与其他频道重叠

## 数值平衡建议

### 频道时间规划（30 分钟）

| 阶段 | 时间 | 建议 |
|------|------|------|
| 早期 | 0-3 min | 1-2 种小怪，间隔 1.0-1.5s |
| 中期 | 3-10 min | 新增 2-3 种小怪，间隔 0.6-1.0s |
| 中后期 | 10-20 min | 更强种类登场，多频道重叠 |
| 后期 | 20-30 min | 高密度，多 Boss |
| 终局 | 30 min | 死神出现 |

### Boss 配置建议
- `SpawnCount = 1`（只出 1 只）
- `HPScale = 5.0-20.0`
- `SpeedScale = 0.3-0.7`（慢但有威胁）
- `DamageScale = 2.0-6.0`
- `IsTreasureChest = TRUE`

## 使用步骤

### 1. 创建敌人预制体映射

1. 在 Unity 中，右键 `Assets/Art/Config` 文件夹
2. 选择 **Create → VampireSurvivorLike → EnemyPrefabMapping**
3. 命名为 `EnemyPrefabMapping`
4. 选中新创建的文件，在 Inspector 中右键选择 **Auto Load Enemy Prefabs** 自动加载所有敌人预制体

### 2. 配置 EnemyGenerator

1. 在场景中找到 `EnemyGenerator` 对象
2. 在 Inspector 中将 `EnemyPrefabMapping` 拖入 **Prefab Mapping** 字段

### 3. 编辑 CSV 配置

1. 使用 Excel 或 WPS 打开 `Assets/StreamingAssets/Config/EnemyWaveConfig.csv`
2. 按频道模式配置各行
3. 保存时选择 **CSV UTF-8（逗号分隔）** 格式

## 注意事项

1. CSV 文件必须使用 **UTF-8** 编码保存
2. EnemyPrefabName 必须与 EnemyPrefabMapping 中的 Name **完全匹配**
3. WebGL 平台完全支持异步加载
4. 修改 CSV 后无需重新打包，直接替换文件即可
5. Active 列设为 FALSE 可临时禁用某个频道
6. 难度递增是自动的，CSV 中的倍率只是**基础值**

## 文件结构

```
Assets/
├── StreamingAssets/
│   └── Config/
│       ├── EnemyWaveConfig.csv        # 敌人时间轴配置表
│       └── AbilityConfig.csv          # 技能属性配置表
├── Scripts/
│   ├── Config/
│   │   ├── EnemyWaveConfigLoader.cs   # CSV 加载器（SpawnChannelConfigLoader）
│   │   ├── AbilityConfigLoader.cs     # 技能 CSV 加载器
│   │   ├── EnemyPrefabMapping.cs      # 预制体映射
│   │   ├── LevelConfig.cs             # 已废弃（保留空壳）
│   │   └── Config.cs                  # 难度公式常量
│   └── Game/
│       └── EnemyGenerator.cs          # 时间轴控制器 + 敌人生成器
└── Art/
    └── Config/
        └── EnemyPrefabMapping.asset   # 预制体映射实例
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

## Q: 如何临时禁用某个频道？
A: 将 `Active` 列设为 `FALSE` 即可。

## Q: 难度递增如何调整？
A: 修改 `Assets/Scripts/Config/Config.cs` 中的增长常量（`HPGrowthPerMinute` 等）。CSV 中的倍率仅为基础值。
