# Boss技能系统使用指南

## 概述

Boss技能系统允许你为不同的Boss配置独特的技能组合，使每个Boss都有自己的战斗风格和威胁特点。

## 文件结构

```
Assets/Scripts/Game/Enemy/BossSkills/
├── IBossSkill.cs           # 技能接口
├── BossSkillBase.cs        # 技能基类
├── DashSkill.cs            # 冲刺技能
├── AreaAttackSkill.cs      # 范围弹幕技能
├── SummonSkill.cs          # 召唤小怪技能
├── SpinAttackSkill.cs      # 旋转攻击技能
└── BossProjectile.cs       # 弹幕投射物
```

## Boss类型

在 `EnemyMiniBoss` 组件的 Inspector 中可选择以下 Boss 类型：

| 类型 | 描述 | 技能组合 |
|------|------|----------|
| **Dasher** | 冲刺型 | 强化冲刺 + 快速冲刺 |
| **Shooter** | 弹幕型 | 8弹幕2波 + 12弹幕3波 |
| **Summoner** | 召唤型 | 4只召唤 + 6只召唤 + 防御冲刺 |
| **Berserker** | 狂战士型 | 旋转攻击 + 冲刺 |
| **Hybrid** | 混合型 | 所有技能（最强） |

## 技能详解

### 1. DashSkill（冲刺技能）

Boss向玩家方向高速冲刺。

**参数：**
- `cooldown`: 冷却时间（秒）
- `warningDuration`: 预警闪烁时间（秒）
- `dashSpeedMultiplier`: 冲刺速度倍率
- `triggerDistance`: 触发距离（与玩家距离小于此值时触发）

**行为流程：**
1. 玩家进入触发距离
2. Boss停止移动，红白闪烁预警（频率逐渐加快）
3. 预警结束后向玩家方向高速冲刺
4. 冲刺超过目标距离后短暂停顿
5. 恢复追踪玩家

### 2. AreaAttackSkill（范围弹幕技能）

Boss发射环形弹幕攻击周围。

**参数：**
- `cooldown`: 冷却时间
- `chargeDuration`: 蓄力时间
- `projectileCount`: 每波弹幕数量
- `projectileSpeed`: 弹幕飞行速度
- `waveCount`: 发射波次数
- `waveInterval`: 波次间隔
- `triggerDistance`: 触发距离

**行为流程：**
1. 玩家进入触发距离
2. Boss停止移动，橙色发光并脉冲放大（蓄力）
3. 向周围发射环形弹幕
4. 可连续发射多波（每波角度偏移）

**注意：** 需要在 Inspector 中配置 `BossProjectilePrefab`

### 3. SummonSkill（召唤技能）

Boss召唤小怪援助战斗。

**参数：**
- `cooldown`: 冷却时间
- `summonCount`: 召唤数量
- `summonRadius`: 召唤生成半径
- `triggerHPPercent`: 血量百分比触发阈值

**行为流程：**
1. Boss血量低于阈值时触发
2. 紫色发光并脉冲
3. 在周围随机位置生成小怪

**注意：** 需要在 Inspector 中配置 `MinionPrefab`

### 4. SpinAttackSkill（旋转攻击技能）

Boss原地旋转并追踪玩家造成范围伤害。

**参数：**
- `cooldown`: 冷却时间
- `chargeTime`: 蓄力时间
- `spinDuration`: 旋转持续时间
- `spinSpeed`: 旋转速度（度/秒）
- `moveSpeed`: 旋转时移动速度
- `damageRadius`: 伤害判定半径
- `damagePerSecond`: 每秒伤害
- `triggerDistance`: 触发距离

**行为流程：**
1. 玩家进入触发距离
2. Boss收缩变黄（蓄力）
3. 放大并开始高速旋转
4. 旋转期间追踪玩家，接触造成持续伤害

## 配置步骤

### 1. 配置现有 EnemyMiniBoss 预制体

1. 打开 Boss 预制体
2. 设置 **Boss Type** 选择技能组合
3. 调整 **Health**（建议 200+）
4. 如果使用 Shooter 类型，配置 **Boss Projectile Prefab**
5. 如果使用 Summoner 类型，配置 **Minion Prefab**

### 2. 创建弹幕预制体

1. 创建新的 2D 对象（如圆形 Sprite）
2. 添加 `Rigidbody2D`（Kinematic 模式）
3. 添加 `CircleCollider2D`（勾选 Is Trigger）
4. 添加 `BossProjectile` 脚本
5. 设置 Layer 为敌人攻击层
6. 保存为预制体

### 3. CSV 配置优化

Boss 在 CSV 中的配置已优化：

```csv
# 早期Boss - 每30秒生成1个，血量×10
小Boss,TRUE,EnemyMiniBoss,30,30,10,0.6,2,1.2,TRUE,0,0,0,0

# 中期Boss - 每40-60秒生成1个，血量×20-40
骷髅Boss,TRUE,EnemyMiniBoss,40,35,20,0.4,3,0.8,TRUE,0,0,0,0

# 终极Boss - 每90-120秒生成1个，血量×80-120
终极Boss,TRUE,EnemyMiniBoss,90,60,80,0.2,6,0.4,TRUE,0,0,0,0
```

**关键调整：**
- `GenerateDuration`: 从 2-3秒 提升到 30-120秒（大幅降低生成频率）
- `HPScale`: 从 2-20 提升到 10-120（大幅提高血量）
- `SpeedScale`: 降低到 0.15-0.6（Boss移动更慢但技能更危险）
- `DamageScale`: 提升到 2-8（Boss伤害更高）

## 掉落配置

Boss 掉落已从固定宝箱改为概率掉落：

```csharp
// Inspector 中可配置
public bool DropTreasureChest = true;     // 是否启用宝箱掉落
public float TreasureChestDropRate = 0.3f; // 宝箱掉落概率（30%）
```

未掉落宝箱时会掉落普通道具（高概率经验和金币）。

## 扩展新技能

1. 创建新类继承 `BossSkillBase`
2. 实现 `OnExecuteStart`、`OnExecuteUpdate`、`OnExecuteEnd`
3. 在 `EnemyMiniBoss.InitializeSkills()` 中添加到对应 Boss 类型
4. （可选）创建新的 BossType 枚举值

**示例：**

```csharp
public class TeleportSkill : BossSkillBase
{
    public override string SkillName => "传送";
    public override float Cooldown => 10f;
    
    protected override void OnExecuteStart()
    {
        // 传送到玩家附近
        var randomOffset = Random.insideUnitCircle * 3f;
        Boss.transform.position = Player.Default.transform.position + 
            new Vector3(randomOffset.x, randomOffset.y, 0);
    }
    
    protected override void OnExecuteUpdate()
    {
        // 传送是瞬发技能
        EndExecution();
    }
}
```

## 数值建议

| 阶段 | 血量 | 伤害倍率 | 生成间隔 | 宝箱概率 |
|------|------|----------|----------|----------|
| 早期 | 200-500 | 1.5-2 | 30-45秒 | 50% |
| 中期 | 500-1000 | 2-3 | 40-60秒 | 40% |
| 后期 | 1000-2000 | 3-5 | 60-90秒 | 30% |
| 终极 | 2000-5000 | 5-8 | 90-120秒 | 25% |
