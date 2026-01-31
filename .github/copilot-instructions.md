# Copilot Instructions - Vampire Survivor-like

## 项目概述

Unity 2D (URP 2D) 类 Vampire Survivors 游戏原型。使用 Unity 2022.3.x LTS + QFramework 框架。

## 架构概览

### 核心分层

- **Global** ([Assets/Scripts/Global/Global.cs](Assets/Scripts/Global/Global.cs)) - 全局状态管理，使用 `BindableProperty<T>` 实现响应式数据绑定，通过 `[RuntimeInitializeOnLoadMethod]` 自动初始化
- **Config** ([Assets/Scripts/Config/Config.cs](Assets/Scripts/Config/Config.cs)) - 静态常量配置（初始伤害、速度等）
- **System** - 各业务系统（继承 `AbstractSystem`），如 `ExpUpgradeSystem`、`AchievementSystem`、`SaveSystem`
- **Game** - 游戏逻辑：角色、敌人、武器能力、掉落物
- **UI** - 面板脚本（继承 `UIPanel`），通过 `UIKit.OpenPanel<T>()` / `ClosePanel<T>()` 管理

### 关键设计模式

**ViewController + Designer 模式**：
- 业务逻辑写在 `XXX.cs`（如 `Player.cs`）
- Unity 引用绑定由 QFramework 自动生成到 `XXX.Designer.cs`
- 类声明使用 `partial class` 连接两部分

```csharp
// Player.cs - 手写业务逻辑
public partial class Player : ViewController { ... }

// Player.Designer.cs - 自动生成的引用
public partial class Player {
    public UnityEngine.Animator Sprite;
    public CircleCollider2D HurtBox;
}
```

**响应式数据绑定**：
```csharp
Global.HP.RegisterWithInitValue(hp => {
    // UI 更新逻辑
}).UnRegisterWhenGameObjectDestroyed(gameObject);
```

## 代码规范

### 命名空间

所有脚本使用 `namespace VampireSurvivorLike`

### 碰撞检测

使用合并的 `HitHurtBox` 组件，通过 `Owner` 字段判断归属：
```csharp
HurtBox.OnTriggerEnter2DEvent(col => {
    var hitHurtBox = col.GetComponent<HitHurtBox>();
    if (hitHurtBox?.Owner.CompareTag("Enemy")) { ... }
});
```

### 伤害计算

统一通过 `DamageSystem.CalculateDamage()` 处理暴击和伤害倍率

### 掉落物

继承 `PowerUp` 抽象类，实现 `Execute()` 方法，`FlyingToPlayer` 属性控制吸取动画

## 系统交互

### 升级系统

- 经验达标 → `Global.Level` 变化 → `ExpUpgradeSystem.Roll()` 生成三选一
- 武器配对关系定义在 `ExpUpgradeSystem.Pairs` 字典
- 宝箱触发合成检查 `PairedProperties`

### 波次生成

`LevelConfig` (ScriptableObject) 配置 `EnemyWaveGroup` → `EnemyWave` 列表，由 `EnemyGenerator` 按时间队列生成

### 存档

`PlayerPrefs` 持久化，`Global.AutoInit()` 中注册自动保存回调

## 场景

- `Assets/Scenes/GameStart.unity` - 主菜单入口
- `Assets/Scenes/Game.unity` - 战斗场景
- `Assets/Scenes/TestMaxEnemyCount.unity` - 性能压测

## 音效

```csharp
AudioKit.PlaySound(Sfx.WALK, loop: true);  // 循环
AudioKit.PlaySound("Hit");                  // 单次
```

## 快速开发指南

1. **新增武器能力**：在 `Assets/Scripts/Game/Ability/` 创建继承 `ViewController` 的脚本，参考 `SimpleSword.cs`
2. **新增掉落物**：在 `Assets/Scripts/Game/PowerUp/` 创建继承 `PowerUp` 的脚本
3. **新增升级项**：在 `ExpUpgradeSystem.ResetData()` 中调用 `Add(new ExpUpgradeItem()...)`
4. **新增成就**：在 `AchievementSystem.OnInit()` 中调用 `Add(new AchievementItem()...)`
5. **新增 UI 面板**：继承 `UIPanel`，使用 `UIKit.OpenPanel<T>()` 打开

## 注意事项

- 修改 `Global` 中的 `BindableProperty` 时，确保对应的 `Register` 回调已正确取消注册
- Designer 文件由 QFramework 自动生成，**不要手动修改**
- 暂停/恢复游戏使用 `Time.timeScale`
