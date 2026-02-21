# 内容扩展方案 —— 基于 rpgItems 素材表与吸血鬼幸存者系统研究

> 版本：v0.11-draft  
> 日期：2026-02-21  
> 状态：方案设计阶段

---

## 0. 方案概述

本方案基于两个核心输入：

1. **rpgItems.png 素材表**（8×8 = 64 格 16×16 像素精灵），涵盖药水、食物、盾牌、武器、饰品等丰富的 RPG 道具图标。
2. **《吸血鬼幸存者》系统研究报告**，提炼出武器进化、被动道具配对、掉落物、属性成长等核心玩法机制。

目标是利用这些素材，在现有游戏系统（5 武器 + 5 被动 + 6 掉落物）基础上，进行**有节制的、可落地的**内容扩展，使游戏内容量达到约 **10 武器 + 10 被动 + 8+ 掉落物** 的规模，并引入**装备系统**和**武器进化**的深度。

---

## 1. 素材映射表（rpgItems.png → 游戏用途）

以下为 rpgItems.png 中每个精灵的推荐用途。每格 16×16 像素，按行列编号。

### 核对结论（2026-02-21）

已按 `Assets/Art/Sprite/rpgItems.png.meta` 逐项核对：`rpgItems_0~rpgItems_63` 与 `Exp` 的切图编号和坐标映射均正确（8×8，16px）。

本轮修正仅针对已确认错误项：

1. `rpgItems_19`（柠檬）在本方案中的 Buff 时长统一为 **8 秒**（不再写 5 秒）。
2. `simple_axe` 行为统一为：**普通态有限穿透，进化后无限穿透**（不再写默认穿透所有敌人）。

### 本期（Phase1）最小素材映射表（已确认）

| 精灵名 | 位置 | 用途 | 备注 |
|---|---|---|---|
| `rpgItems_37` | (5,4) | 斧头武器图标（`simple_axe`） | UI 图标 |
| `rpgItems_38` | (6,4) | 斧头进化图标（`SuperAxe`） | UI 进化图标 |
| `rpgItems_63` | (7,7) | 斧头投射物精灵 | 场景内飞斧弹体 |
| `copper_helmet` | (4,0) | 护甲被动（`armor`）图标 | UI 图标 |
| `rpgItems_3` | (3,0) | 烛台被动（`candelabrador`）图标 | UI 图标 |
| `rpgItems_32` | (0,4) | 烤肉掉落物（`roast_meat`） | 回复 3 HP |
| `rpgItems_19` | (3,2) | 柠檬掉落物（`lemon_buff`） | 伤害 +50%，持续 8 秒 |

### 第 0 行（y=112） —— 药水 & 防具

| 精灵名 | 位置 | 外观 | 建议用途 | 优先级 |
|---|---|---|---|---|
| `rpgItems_0` | (0,0) | 红色药水 | **回血掉落物（大）** — 替代/升级现有 RecoverHP 精灵 | ★★★ |
| `rpgItems_1` | (1,0) | 绿色药水 | **新被动：再生（Regeneration）** — 每秒回血 | ★★☆ |
| `rpgItems_2` | (2,0) | 蓝色药水 | **新被动：冷却缩减（Cooldown）** — 减少武器攻击间隔 | ★★★ |
| `rpgItems_3` | (3,0) | 火炬/蜡烛 | **新被动：烛台（Candelabrador）** — 增加武器范围 | ★★☆ |
| `copper_helmet` | (4,0) | 铜头盔 | **新被动：护甲（Armor）** — 降低受到伤害 | ★★★ |
| `rpgItems_5` | (5,0) | 皮甲 | **新掉落物：护甲碎片** — 临时减伤 | ★☆☆ |
| `rpgItems_6` | (6,0) | 冰晶甲 | **护甲进化后图标** — 高级护甲视觉 | ★☆☆ |
| `rpgItems_7` | (7,0) | 白铠甲 | **成就/UI 装饰** — 全防具收集成就图标 | ☆☆☆ |

### 第 1 行（y=96） —— 食物 & 靴子

| 精灵名 | 位置 | 外观 | 建议用途 | 优先级 |
|---|---|---|---|---|
| `rpgItems_8` | (0,1) | 面包 | **新掉落物：面包** — 回复 1 HP（替代烤鸡概念） | ★★☆ |
| `rpgItems_9` | (1,1) | 蘑菇/蒜 | **新被动：大蒜（Garlic）** — 光环持续伤害 + 击退 | ★★★ |
| `rpgItems_10` | (2,1) | 草莓 | **新掉落物：草莓** — 获得 5 秒加速 buff | ★☆☆ |
| `rpgItems_11` | (3,1) | 樱桃对 | **新掉落物：樱桃** — 全屏清怪（类似炸弹但不掉落伤害） | ★★☆ |
| `rpgItems_12` | (4,1) | 皮靴 | **移动速度被动图标优化** — 替换现有 movement_icon | ★★☆ |
| `rpgItems_13` | (5,1) | 蓝靴 | **超级移速被动图标** — 合成后图标 | ★☆☆ |
| `rpgItems_14` | (6,1) | 铁裤 | **备用 UI 素材** | ☆☆☆ |
| `rpgItems_15` | (7,1) | 白裤 | **备用 UI 素材** | ☆☆☆ |

### 第 2 行（y=80） —— 水果 & 盾牌

| 精灵名 | 位置 | 外观 | 建议用途 | 优先级 |
|---|---|---|---|---|
| `rpgItems_16` | (0,2) | 苹果 | **经验掉落物视觉变体** — 大经验球 | ★☆☆ |
| `rpgItems_17` | (1,2) | 青苹果 | **大蒜被动替代图标**（若不用蘑菇） | ★☆☆ |
| `rpgItems_18` | (2,2) | 蓝莓 | **新掉落物：蓝莓** — 回复 2 HP | ★☆☆ |
| `rpgItems_19` | (3,2) | 柠檬 | **新掉落物：柠檬** — 8 秒伤害提升 buff | ★★☆ |
| `rpgItems_20` | (4,2) | 铜盾 | **新被动：格挡（Block）** — 概率完全抵消伤害 | ★★☆ |
| `rpgItems_21` | (5,2) | 铜手套 | **新被动：攻击力（Attack Power）** — 提升基础攻击力 | ★☆☆ |
| `rpgItems_22` | (6,2) | 铁盾 | **格挡被动 Lv2 图标** | ★☆☆ |
| `rpgItems_23` | (7,2) | 铁手套 | **攻击力被动 Lv2 图标** | ★☆☆ |

### 第 3 行（y=64） —— 水果 & 杂项

| 精灵名 | 位置 | 外观 | 建议用途 | 优先级 |
|---|---|---|---|---|
| `Exp` | (0,3) | 红水晶 | **已使用** — 经验掉落物（当前名称） | — |
| `rpgItems_25` | (1,3) | 绿水晶 | **新掉落物：绿水晶** — 小量经验 buff | ★☆☆ |
| `rpgItems_26` | (2,3) | 蓝水晶 | **备用掉落物素材** | ☆☆☆ |
| `rpgItems_27` | (3,3) | 黄水晶 | **备用 buff 掉落素材** | ☆☆☆ |
| `rpgItems_28` | (4,3) | 铜铲子 | **备用**| ☆☆☆ |
| `rpgItems_29` | (5,3) | 铜十字镐 | **备用装备素材** | ☆☆☆ |
| `rpgItems_30` | (6,3) | 铁铲子 | **备用装备素材** | ☆☆☆ |
| `rpgItems_31` | (7,3) | 铁十字镐 | **备用装备素材** | ☆☆☆ |

### 第 4 行（y=48） —— 肉 & 斧头/剑

| 精灵名 | 位置 | 外观 | 建议用途 | 优先级 |
|---|---|---|---|---|
| `rpgItems_32` | (0,4) | 酒 | **新掉落物：酒** — 回复 3 HP（稀有掉落） | ★★★ |
| `rpgItems_33` | (1,4) | 肉棒 | **已使用** — 回血道具 | ★★★ |
| `rpgItems_34` | (2,4) | 金锭 | ** ** —  | ★☆☆ |
| `rpgItems_35` | (3,4) | 骨头 | **** —  | ★★★ |
| `rpgItems_36` | (4,4) | 铜战锤 | **** | ★★☆ |
| `rpgItems_37` | (5,4) | 铜剑 | **新武器：斧头（Axe）** — 抛物线飞斧（激活已有 SimpleAxe） | ★★★ |
| `rpgItems_38` | (6,4) | 铁战锤 | **** | ★☆☆ |
| `rpgItems_39` | (7,4) | 铁剑 | **备用武器素材** | ☆☆☆ |

### 第 5 行（y=32） —— 饰品 & 锤

| 精灵名 | 位置 | 外观 | 建议用途 | 优先级 |
|---|---|---|---|---|
| `rpgItems_40` | (0,5) | 饼干 | **新掉落物：饼干** — 获得 10 秒经验翻倍 buff | ★★☆ |
| `rpgItems_41` | (1,5) | 金币 | **** — 替换现有金币精灵 | ★★☆ |
| `rpgItems_42` | (2,5) | 银币 | **** — 脚下留毒圈 | ★★☆ |
| `rpgItems_43` | (3,5) | 铜币 | **** | ☆☆☆ |
| `rpgItems_44` | (4,5) | 铜战斧 | **** — 环绕+甩出攻击 | ★★☆ |
| `rpgItems_45` | (5,5) | 铜刀 | **** | ★☆☆ |
| `rpgItems_46` | (6,5) | 铁战斧 | **** — 自动瞄准弹幕 | ★★★ |
| `rpgItems_47` | (7,5) | 铁刀 | **** | ★★☆ |

### 第 6 行（y=16） —— 远程武器

| 精灵名 | 位置 | 外观 | 建议用途 | 优先级 |
|---|---|---|---|---|
| `rpgItems_48` | (0,6) | 炸弹（燃烧） | **已使用** — | ★★☆ |
| `rpgItems_49` | (1,6) | 飞镖 | **新武器：飞镖（Boomerang）** — 飞出后返回 | ★★★ |
| `rpgItems_50` | (2,6) | 打火石 | **** | ★★☆ |
| `rpgItems_51` | (3,6) | 铁砧 | **** —  | ★★★ |
| `rpgItems_52` | (4,6) | 铜弓 | **** —  | ★★☆ |
| `rpgItems_53` | (5,6) | 铜箭支 | **** | ★☆☆ |
| `rpgItems_54` | (6,6) | 铁弓 | **** —  | ★★☆ |
| `rpgItems_55` | (7,6) | 铁箭支 | **** | ★☆☆ |

### 第 7 行（y=0） —— 戒指 & 特殊物品

| 精灵名 | 位置 | 外观 | 建议用途 | 优先级 |
|---|---|---|---|---|
| `rpgItems_56` | (0,7) | 火焰戒指 | **新被动：烈焰之心（Fire Heart）** — 增加伤害倍率 | ★★★ |
| `rpgItems_57` | (1,7) | 深蓝戒指 | **新被动：幸运（Luck）** — 增加掉落品质 | ★★☆ |
| `rpgItems_58` | (2,7) | 金钥匙 | **** —  | ★★☆ |
| `rpgItems_59` | (3,7) | 铁钥匙 | **** — | ★☆☆ |
| `rpgItems_60` | (4,7) | 铜弩 | **** —  | ★★☆ |
| `rpgItems_61` | (5,7) | 铜匕首 | **** —  | ★★☆ |
| `rpgItems_62` | (6,7) | 铁弩 | **** —  | ★☆☆ |
| `rpgItems_63` | (7,7) | 铁匕首 | **** —  | ★★☆ |

---

## 2. 扩展内容设计

### 2.1 新增武器（5 种） → 总计 10 武器

下表列出 5 种新增武器，均可利用 rpgItems.png 中的素材。设计参考了《吸血鬼幸存者》的武器多样性理念（直线型、环绕型、AOE 型、反弹型、延迟型）。

| # | 武器名 | Key | 类型 | 精灵 | 行为描述 | 初始属性 | 配对被动 | 进化产物 |
|---|---|---|---|---|---|---|---|---|
| 6 | **斧头** | `simple_axe` | 抛物线 | `rpgItems_37` / 弹体 `rpgItems_63` | 向上抛出飞斧，受重力影响沿抛物线落下（普通态有限穿透，进化后无限穿透） | 伤害 8, 间隔 1.2s, 数量 1 | 烛台 `candelabrador` | **死亡旋风 (Death Spiral)** — 360° 大范围旋转斧 |
| 7 | **魔杖** | `magic_wand` | 自动瞄准 | `rpgItems_46` / 进化 `rpgItems_47` | 自动向最近敌人发射魔弹，击退效果 | 伤害 4, 间隔 0.8s, 数量 1 | 空日之书 `empty_tome` | **圣杖 (Holy Staff)** — 连续射击 + 穿透 |
| 8 | **弓箭** | `simple_bow` | 穿透射线 | `rpgItems_52` / 弹体 `rpgItems_60` | 向最近敌人方向射出高速箭矢，穿透 3 个敌人 | 伤害 6, 间隔 1.0s, 数量 2, 穿透 3 | 护甲 `armor` | **精灵弓 (Elven Bow)** — 箭矢分裂 + 追踪 |
| 9 | **飞镖** | `boomerang` | 往返型 | `rpgItems_49` | 向最近方向投掷飞镖，飞出后返回，来回均造成伤害 | 伤害 5, 间隔 1.5s, 数量 1 | 幸运 `luck` | **回旋刃 (Razor Boomerang)** — 多次往返 + 范围增大 |
| 10 | **圣水** | `holy_water` | 地面AOE | `rpgItems_42` | 在脚下生成持续伤害水圈，敌人踩入持续受伤+减速 | 伤害 2/tick, 间隔 3.0s, 持续 2s | 吸引器 `attractorb` | **短绒 (La Borra)** — 水圈随角色移动 + 范围扩大 |

#### 斧头武器详细设计（激活已有 SimpleAxe 代码）

现有代码 `SimpleAxe.cs` 和 `PooledAxeProjectile.cs` 已实现基础逻辑但未注册到升级系统。扩展步骤：

1. 在 `Global.cs` 添加：`SimpleAxeUnlocked`、`SimpleAxeDamage`、`SimpleAxeDuration`、`SimpleAxeCount`、`SuperAxe`
2. 在 `Config.cs` 添加：`InitSimpleAxeDamage=8`、`InitSimpleAxeDuration=1.2f`、`InitSimpleAxeCount=1`
3. 在 `ExpUpgradeSystem.ResetData()` 注册升级项和 Pairs
4. 在 `AbilityController` 中添加斧头的 Show/Hide 监听
5. 替换斧头精灵为 `rpgItems_37`（战斧图标）和 `rpgItems_63`（暗战斧弹体）

#### 升级路径设计（以斧头为例）

| 等级 | 效果 | 描述 |
|---|---|---|
| Lv1 | 解锁斧头 | 向上投掷飞斧，沿抛物线落下 |
| Lv2 | 伤害 +3 | — |
| Lv3 | 数量 +1 | — |
| Lv4 | 伤害 +3, 间隔 -0.1s | — |
| Lv5 | 穿透 +1 | — |
| Lv6 | 伤害 +4, 数量 +1 | — |
| Lv7 | 间隔 -0.2s | — |
| Lv8 | 伤害 +5 | — |
| Lv9 | 数量 +1, 穿透 +1 | — |
| Lv10 | 伤害 +5, 间隔 -0.2s | — |

---

### 2.2 新增被动道具（5 种） → 总计 10 被动 + 1 无配对

设计原则：每个新被动都与一个新/旧武器配对，形成进化组合。

| # | 被动名 | Key | 精灵 | 效果 | MaxLevel | 每级增量 | 配对武器 |
|---|---|---|---|---|---|---|---|
| 7 | **护甲** | `armor` | `copper_helmet` | 降低受到伤害（-1/级，最高-5） | 5 | -1 伤害 | ↔ `simple_bow` |
| 8 | **烛台** | `candelabrador` | `rpgItems_3` | 增加所有武器范围（+10%/级） | 5 | +10% 范围 | ↔ `simple_axe` |
| 9 | **空日之书** | `empty_tome` | `rpgItems_51` | 减少所有武器冷却时间（-8%/级） | 5 | -8% 冷却 | ↔ `magic_wand` |
| 10 | **幸运** | `luck` | `rpgItems_57` | 增加掉落品质和暴击率（+5%/级） | 5 | +5% 幸运 | ↔ `boomerang` |
| 11 | **吸引器** | `attractorb` | `rpgItems_58` | 增加拾取范围（替换现有 `collectable_area_radius`） | 5 | +0.8 半径 | ↔ `holy_water` |

> **注**：现有 `simple_collectable_area_radius` 无配对，可将其改造为 `attractorb`，既保留功能又增加了进化价值。

---

### 2.3 新增掉落物（3 种） → 总计 9 掉落物

| # | 掉落物名 | Key | 精灵 | 效果 | 生成条件 |
|---|---|---|---|---|---|
| 7 | **烤肉** | `roast_meat` | `rpgItems_32` | 回复 3 HP（稀有回复） | 精英/Boss 掉落，概率 2% |
| 8 | **樱桃** | `cherry` | `rpgItems_11` | 全屏伤害（弱化版炸弹，伤害 = 当前攻击力×2） | Boss 掉落，概率 5% |
| 9 | **柠檬** | `lemon_buff` | `rpgItems_19` | 获得 8 秒伤害 +50% buff | 精英掉落，概率 3% |

#### 掉落物实现参考

```csharp
// RoastMeat.cs — 继承 PowerUp
public class RoastMeat : PowerUp
{
    public override bool FlyingToPlayer => true;
    
    protected override void Execute()
    {
        var healAmount = 3;
        Global.HP.Value = Mathf.Min(Global.HP.Value + healAmount, Global.MaxHP.Value);
        AudioKit.PlaySound("Heal");
    }
}
```

---

### 2.4 武器进化全谱系（The Grand Evolution Table）

参考《吸血鬼幸存者》的进化系统设计，以下为完整的进化配对表（含现有 + 新增）：

| 基础武器 | 需求被动（Lv1+） | 进化触发条件 | 进化产物 | 进化效果 |
|---|---|---|---|---|
| **剑** `simple_sword` | 暴击 `simple_critical` | 武器 Lv7 + 被动已解锁 + 宝箱 | **超级剑** | 攻击力 ×2，攻击距离 ×2 |
| **飞刀** `simple_knife` | 伤害倍率 `damage_rate` | 同上 | **超级飞刀** | 攻击力 ×2 |
| **守卫剑** `rotate_sword` | 经验值 `simple_exp_percent` | 同上 | **超级守卫剑** | 攻击力 ×2，旋转速度 ×2 |
| **篮球** `basket_ball` | 移动速度 `movement_speed_rate` | 同上 | **超级篮球** | 攻击力 ×2，体型变大 |
| **炸弹** `simple_bomb` | 飞射物 `simple_fly_count` | 同上 | **超级炸弹** | 每 15 秒自动爆炸 |
| **斧头** `simple_axe` | 烛台 `candelabrador` | 同上 | **死亡旋风** | 360° 全屏穿透旋转斧 |
| **魔杖** `magic_wand` | 空日之书 `empty_tome` | 同上 | **圣杖** | 连射 + 穿透 + 击退增强 |
| **弓箭** `simple_bow` | 护甲 `armor` | 同上 | **精灵弓** | 箭矢 ×3 分裂 + 轻微追踪 |
| **飞镖** `boomerang` | 幸运 `luck` | 同上 | **回旋刃** | 3 次往返 + 范围 ×2 |
| **圣水** `holy_water` | 吸引器 `attractorb` | 同上 | **短绒** | 水圈跟随角色 + 范围扩大 |

---

## 3. 属性系统扩展

### 3.1 新增 Global 属性

```csharp
// === 斧头 ===
public static BindableProperty<bool> SimpleAxeUnlocked = new(false);
public static BindableProperty<float> SimpleAxeDamage = new(Config.InitSimpleAxeDamage);
public static BindableProperty<float> SimpleAxeDuration = new(Config.InitSimpleAxeDuration);
public static BindableProperty<int> SimpleAxeCount = new(Config.InitSimpleAxeCount);
public static BindableProperty<bool> SuperAxe = new(false);

// === 魔杖 ===
public static BindableProperty<bool> MagicWandUnlocked = new(false);
public static BindableProperty<float> MagicWandDamage = new(Config.InitMagicWandDamage);
public static BindableProperty<float> MagicWandDuration = new(Config.InitMagicWandDuration);
public static BindableProperty<int> MagicWandCount = new(Config.InitMagicWandCount);
public static BindableProperty<bool> SuperMagicWand = new(false);

// === 弓箭 ===
public static BindableProperty<bool> SimpleBowUnlocked = new(false);
public static BindableProperty<float> SimpleBowDamage = new(Config.InitSimpleBowDamage);
public static BindableProperty<float> SimpleBowDuration = new(Config.InitSimpleBowDuration);
public static BindableProperty<int> SimpleBowCount = new(Config.InitSimpleBowCount);
public static BindableProperty<int> SimpleBowPierce = new(Config.InitSimpleBowPierce);
public static BindableProperty<bool> SuperBow = new(false);

// === 飞镖 ===
public static BindableProperty<bool> BoomerangUnlocked = new(false);
public static BindableProperty<float> BoomerangDamage = new(Config.InitBoomerangDamage);
public static BindableProperty<float> BoomerangDuration = new(Config.InitBoomerangDuration);
public static BindableProperty<int> BoomerangCount = new(Config.InitBoomerangCount);
public static BindableProperty<bool> SuperBoomerang = new(false);

// === 圣水 ===
public static BindableProperty<bool> HolyWaterUnlocked = new(false);
public static BindableProperty<float> HolyWaterDamage = new(Config.InitHolyWaterDamage);
public static BindableProperty<float> HolyWaterDuration = new(Config.InitHolyWaterDuration);
public static BindableProperty<float> HolyWaterTickInterval = new(Config.InitHolyWaterTickInterval);
public static BindableProperty<bool> SuperHolyWater = new(false);

// === 新被动属性 ===
public static BindableProperty<int> ArmorValue = new(0);           // 护甲减伤
public static BindableProperty<float> AreaMultiplier = new(1.0f);  // 烛台-范围倍率
public static BindableProperty<float> CooldownReduction = new(0);  // 空日之书-冷却缩减
public static BindableProperty<float> LuckValue = new(0);          // 幸运值
```

### 3.2 新增 Config 常量

```csharp
// === 斧头 ===
public const float InitSimpleAxeDamage = 8;
public const float InitSimpleAxeDuration = 1.2f;
public const int InitSimpleAxeCount = 1;

// === 魔杖 ===
public const float InitMagicWandDamage = 4;
public const float InitMagicWandDuration = 0.8f;
public const int InitMagicWandCount = 1;

// === 弓箭 ===
public const float InitSimpleBowDamage = 6;
public const float InitSimpleBowDuration = 1.0f;
public const int InitSimpleBowCount = 2;
public const int InitSimpleBowPierce = 3;

// === 飞镖 ===
public const float InitBoomerangDamage = 5;
public const float InitBoomerangDuration = 1.5f;
public const int InitBoomerangCount = 1;

// === 圣水 ===
public const float InitHolyWaterDamage = 2;
public const float InitHolyWaterDuration = 3.0f;
public const float InitHolyWaterTickInterval = 0.5f;
```

---

## 4. 波次系统扩展建议

### 4.1 新增波次事件

参考研究报告中的"集群 (Swarms)"和"围墙 (Walls)"概念：

| 时间点 | 事件类型 | 描述 | 敌人类型 | 数量 |
|---|---|---|---|---|
| 07:00 | **幽灵集群** | 30 秒内高密度幽灵涌入，经验获取黄金窗口 | Ghost | 每秒 5 个 |
| 12:00 | **蝙蝠风暴** | 四面蝙蝠高速突进，测试 AOE 能力 | Bat | 每秒 8 个 |
| 16:00 | **方块围墙** | 方块排成环形向玩家收缩 | Block_Yellow | 20 个一次性生成 |
| 22:00 | **精英冲锋** | 高 HP 独眼 + 盗贼混合高速波 | Cyclops+Rouge | 各 10 个 |
| 27:00 | **终局预演** | 所有类型同时出现，密度拉满 | 全部 | 上限 |

### 4.2 EnemyWaveConfig.csv 新增频道示例

```csv
# 幽灵集群事件（7分钟）
channel_name,enemy_type,start_time,end_time,spawn_interval,hp_scale,speed_scale,damage_scale,is_boss,boss_count,has_treasure
幽灵-集群-7分,Ghost,420,450,0.2,0.6,1.2,0.5,false,0,false

# 蝙蝠风暴事件（12分钟）
蝙蝠-风暴-12分,Bat,720,750,0.125,0.8,1.3,0.7,false,0,false

# 方块围墙事件（16分钟）
方块-围墙-16分,Block_Yellow,960,965,0.25,1.5,0.5,1.0,false,0,false
```

---

## 5. 实施路线图

### Phase 1：基础扩展（预计 3-5 天）

**目标**：激活斧头 + 新增 2 个被动 + 2 个掉落物

- [ ] **斧头激活**
  - 修改 `Global.cs` — 添加斧头属性
  - 修改 `Config.cs` — 添加斧头常量
  - 修改 `ExpUpgradeSystem.ResetData()` — 注册斧头升级项
  - 修改 `AbilityController` — 添加斧头监听
  - 更新 `SimpleAxe.cs` — 对接 Global 属性
  - 更新精灵引用 → `rpgItems_37`, `rpgItems_63`
  - 添加本地化 — 所有 CSV 文件

- [ ] **新被动：护甲**
  - 创建 `Global.ArmorValue` 属性
  - 在 `ExpUpgradeSystem` 中注册
  - 修改 `DamageSystem.CalculateDamage()` — 减去护甲值
  - 图标使用 `copper_helmet`

- [ ] **新被动：烛台**
  - 创建 `Global.AreaMultiplier` 属性
  - 各武器的范围/面积计算接入该倍率
  - 图标使用 `rpgItems_3`

- [ ] **新掉落物：烤肉**
  - 创建 `RoastMeat.cs` — 回复 3HP
  - 添加到掉落概率表
  - 精灵使用 `rpgItems_32`

- [ ] **新掉落物：柠檬 buff**
  - 创建 `LemonBuff.cs` — 临时伤害提升
  - 精灵使用 `rpgItems_19`

- [ ] **进化配对**
  - `simple_axe ↔ candelabrador`
  - `simple_bow ↔ armor`（弓箭在 Phase 2）

### Phase 2：武器丰富化（预计 5-7 天）

**目标**：新增魔杖 + 弓箭 + 2 个被动

- [ ] **新武器：魔杖**
  - 创建 `MagicWand.cs` + `PooledMagicBullet.cs`
  - 自动瞄准最近敌人，发射魔弹
  - 精灵使用 `rpgItems_46`

- [ ] **新武器：弓箭**
  - 创建 `SimpleBow.cs` + `PooledArrowProjectile.cs`
  - 高速穿透箭矢，弹体使用 `rpgItems_60`
  - 精灵使用 `rpgItems_52`

- [ ] **新被动：空日之书**
  - 全局攻击间隔缩减
  - 图标使用 `rpgItems_51`

- [ ] **新被动：幸运**
  - 增加掉落品质 + 暴击率加成
  - 图标使用 `rpgItems_57`

- [ ] **进化配对**
  - `magic_wand ↔ empty_tome` → 圣杖
  - `simple_bow ↔ armor` → 精灵弓

### Phase 3：玩法深度（预计 5-7 天）

**目标**：新增飞镖 + 圣水 + 波次事件 + 掉落物

- [ ] **新武器：飞镖**
  - 创建 `Boomerang.cs` + `PooledBoomerangProjectile.cs`
  - 飞出后沿路径返回，来回伤害
  - 精灵使用 `rpgItems_49`

- [ ] **新武器：圣水**
  - 创建 `HolyWater.cs` + `HolyWaterZone.cs`
  - 地面 AOE 持续伤害 + 减速
  - 精灵使用 `rpgItems_42`

- [ ] **新被动：吸引器**（改造现有拾取范围）
  - 重构 `simple_collectable_area_radius` → `attractorb`
  - 添加与圣水的配对关系

- [ ] **波次事件系统**
  - 在 `EnemyGenerator` 中添加事件频道支持
  - 配置集群/围墙波次到 CSV

- [ ] **新掉落物：樱桃**
  - 创建 `Cherry.cs` — 弱化全屏伤害
  - 精灵使用 `rpgItems_11`

- [ ] **进化配对**
  - `boomerang ↔ luck` → 回旋刃
  - `holy_water ↔ attractorb` → 短绒

---

## 6. 精灵图标使用清单

### 确定使用（高优先级）

| 精灵名 | 用途 | 需要操作 |
|---|---|---|
| `rpgItems_37` | 斧头武器图标 | 添加到 Icon SpriteAtlas |
| `rpgItems_63` | 飞斧弹体 | 用作 PooledAxeProjectile 精灵 |
| `copper_helmet` | 护甲被动图标 | 添加到 Icon SpriteAtlas |
| `rpgItems_3` | 烛台被动图标 | 添加到 Icon SpriteAtlas |
| `rpgItems_51` | 空日之书被动图标 | 添加到 Icon SpriteAtlas |
| `rpgItems_32` | 烤肉掉落物精灵 | 用作掉落物 SpriteRenderer |
| `rpgItems_19` | 柠檬 buff 掉落物精灵 | 用作掉落物 SpriteRenderer |
| `rpgItems_46` | 魔杖武器图标 | 添加到 Icon SpriteAtlas |
| `rpgItems_52` | 弓箭武器图标 | 添加到 Icon SpriteAtlas |
| `rpgItems_60` | 箭矢弹体 | 用作 PooledArrowProjectile 精灵 |
| `rpgItems_49` | 飞镖武器图标 | 添加到 Icon SpriteAtlas |
| `rpgItems_42` | 圣水武器图标 | 添加到 Icon SpriteAtlas |
| `rpgItems_57` | 幸运被动图标 | 添加到 Icon SpriteAtlas |
| `rpgItems_58` | 吸引器被动图标 | 添加到 Icon SpriteAtlas |

### 可选使用（低优先级）

| 精灵名 | 用途 |
|---|---|
| `rpgItems_0` | 大号回血掉落替代精灵 |
| `rpgItems_11` | 樱桃掉落物 |
| `rpgItems_41` | 金币掉落物替代精灵 |
| `rpgItems_33` | HP UI 心形图标 |
| `rpgItems_48` | 炸弹图标优化 |
| `rpgItems_12` | 移速被动图标优化（皮靴） |
| `rpgItems_56` | 伤害倍率图标替代（火焰戒指，替代 damage_icon） |
| `rpgItems_35`/`rpgItems_36` | 圣剑武器（如果需要第11种武器） |
| `rpgItems_44`/`rpgItems_45` | 流星锤武器（如果需要第12种武器） |
| `rpgItems_54`/`rpgItems_55` | 长矛武器（如果需要第13种武器） |
| `rpgItems_61` | 骨头反弹武器弹体 |

### 未使用（暂无明确用途）

`rpgItems_1`(绿药水), `rpgItems_5-7`(防具组), `rpgItems_10`(草莓), `rpgItems_13-15`(靴裤组), `rpgItems_16-18`(水果), `rpgItems_20-23`(盾牌组进化), `rpgItems_25-27`(水果), `rpgItems_28-31`(盾牌杂项), `rpgItems_34`(黄金果), `rpgItems_38-39`(斧进化), `rpgItems_40`(饼干), `rpgItems_43`(铜奖章), `rpgItems_47`(白杖进化), `rpgItems_50`(弩进化), `rpgItems_53`(白弓进化), `rpgItems_59`(卷轴), `rpgItems_62`(细剑)

> 这些素材可以在后续版本中用于：装备系统展示、成就图标、商店 UI、角色皮肤道具等。

---

## 7. 数值平衡参考

### 7.1 伤害成长曲线对比

基于《吸血鬼幸存者》研究报告中的"指数级成长 vs 线性压力"理念：

```
玩家 DPS ≈ Σ(武器基础伤害 × 等级加成 × 伤害倍率 × 暴击期望) / 攻击间隔
敌人 EHP ≈ 基础HP × (1 + HPGrowthPerMinute × 分钟数) × HP缩放
```

**目标**：玩家在 15 分钟左右达到"质变点"（第一次进化），20 分钟后进入"收割模式"，25-28 分钟面临后期压力考验。

### 7.2 武器 DPS 基准（Lv1 时）

| 武器 | 基础伤害 | 攻击间隔 | 命中数 | 理论 DPS |
|---|---|---|---|---|
| 剑 | 3 | 1.5s | 3 | 6.0 |
| 飞刀 | 5 | 1.0s | 3 | 15.0 |
| 守卫剑 | 5 | 持续 | 1 | ~8.0 |
| 篮球 | 5 | ~ | 1 | ~5.0 |
| 炸弹 | 10 | 概率 | 全屏 | ~2.0（期望） |
| **斧头** | 8 | 1.2s | 1+穿透 | ~10.0 |
| **魔杖** | 4 | 0.8s | 1 | 5.0 |
| **弓箭** | 6 | 1.0s | 2×3穿透 | 36.0（理论） |
| **飞镖** | 5 | 1.5s | 1×2(来回) | 6.7 |
| **圣水** | 2/tick | 0.5s/tick | AOE | ~16.0（密集） |

> 弓箭理论 DPS 较高但受限于直线范围；圣水受限于脚下范围。实际 DPS 需要根据敌人分布调整。

---

## 8. 技术注意事项

### 8.1 精灵切割与引用

rpgItems.png 已在 Unity Sprite Editor 中切割为 64 个 16×16 的子精灵（spriteMode=2，Multiple）。使用时：

1. **Icon SpriteAtlas**：将需要的子精灵拖入 `Assets/Art/Sprite/UI/Icon/Icon.spriteatlasv2`
2. **ExpUpgradePanel** 通过 `iconAtlas.GetSprite(iconName)` 加载图标
3. **弹体精灵**：在对应武器预制体的 SpriteRenderer 上直接引用子精灵

### 8.2 对象池化

所有新武器的投射物必须使用对象池化模式：
- 继承现有的 `PooledKnifeProjectile` 风格
- 使用 `GameObjectPool` 或 QFramework 的 `SimpleObjectPool`
- 设置合理的初始池大小（弓箭约 20，魔杖约 15）

### 8.3 性能预算

- 维持同屏上限 300 敌人不变
- 新增武器的投射物总量控制在 50 个以内
- 集群波次事件持续时间不超过 30 秒，避免帧率压力

### 8.4 本地化

每个新增武器/被动需要在以下 5 个 CSV 文件中添加条目：
- `upgrade.en.csv`
- `upgrade.zh-Hans.csv`
- `upgrade.zh-Hant.csv`
- `upgrade.ja.csv`
- `upgrade.ko.csv`

---

## 9. 总结

本方案充分利用了 rpgItems.png 中的 **14+ 个高优先级精灵**和 **10+ 个可选精灵**，结合《吸血鬼幸存者》的核心玩法理念，规划了：

| 类别 | 现有数量 | 新增数量 | 扩展后总计 |
|---|---|---|---|
| 武器 | 5 (+1未激活) | +5 | **10** |
| 被动 | 5 +1(无配对) | +5 | **11** |
| 武器进化 | 5 对 | +5 对 | **10 对** |
| 掉落物 | 6 种 | +3 种 | **9 种** |
| 波次事件 | 0 | +5 | **5** |

扩展后游戏将拥有更丰富的Build组合空间（10 武器 × 11 被动 = 大量组合），更多的进化路径选择（10 条进化线），以及更刺激的波次事件节奏。
