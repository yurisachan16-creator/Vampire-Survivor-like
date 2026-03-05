# Vampire Survivor-like

Unity 2D（URP 2D）练习项目：做一个“类 Vampire Survivors”的可游玩原型，包含波次敌人、击杀掉落、经验升级三选一、宝箱合成、永久升级（存档）与成就系统等。

参考学习过程来源（非搬运内容）：https://www.gamepixedu.com/course/133/task/12379/show

## 环境

- Unity：2022.3.62f2c1（LTS）
- 渲染：URP 14.x（2D）
- 主要框架：QFramework（BindableProperty / UIKit / ResKit / AudioKit 等）

## 快速开始

1. 用 Unity Hub 打开本项目。
2. 打开并运行场景：
   - `Assets/Scenes/GameStart.unity`：开始界面/入口
   - `Assets/Scenes/Game.unity`：实际战斗场景
3. 点 Play 即可游玩。

如果要 Build：把上述两个场景加入 Build Settings 的 Scenes In Build。

## 操作说明

- 移动：WASD / 方向键（走路速度会叠加移动速度加成）
- 攻击：自动（根据已解锁武器/被动的配置自动触发）
- 升级：每次升级会暂停游戏并弹出升级界面，点击一个选项选择升级
- 宝箱：拾取后暂停游戏并弹出宝箱面板（用于合成/升级或奖励）

## 玩法与系统概览

### 核心数值

- 经验值/等级：击杀掉落经验球，达到阈值升级
- 时间：用于波次推进、通关判定与成就
- 金币：用于永久升级（并存档）
- 生命值：受伤/回血道具

### 波次与通关

- 按时间生成不同波次的敌人（含普通/精英/Boss）
- 通关条件：最后一波生成完毕，且场上敌人清空

### 升级系统（战斗内）

- 升级触发：经验值达到阈值后提升等级，并弹出升级三选一（暂停）
- 武器/能力：简单剑、守卫剑（环绕）、飞刀、篮球、斧头等
- 被动/属性：暴击、伤害倍率、移动速度、拾取范围、额外经验等

### 配对与超级武器（合成）

- 部分武器与被动存在配对关系
- 满足条件后，通过宝箱触发合成（表现为“超级武器”升级）

### 掉落与道具

- 常规掉落：经验球、金币
- 功能道具：回血、炸弹、吸全屏经验/金币（GetAllExp）、宝箱等
- 掉落概率会受到永久升级与战斗内加成影响

### 永久升级与存档

- 使用金币购买永久加成（例如经验/金币掉落概率、最大生命等）
- 支持存档与次数限制

### 成就系统

- 记录关键指标（例如等级、时间、累计完成情况等）
- 在开始界面/成就面板展示

## 性能与压测

为验证“同屏大量对象”的可用性，项目提供了若干优化与测试场景：

- 碰撞脚本合并：将 `HitBox` 与 `HurtBox` 合并为 `HitHurtBox`，减少脚本数量与组件查找
- GetAllExp 优化：一次触发可让大量经验/金币道具飞向玩家并被吸收

### 测试场景

- `Assets/Scenes/TestMaxEnemyCount.unity`：敌人数量压测
- `Assets/Scenes/TestMaxPowerUpCount.unity`：道具数量压测（会生成大量掉落并触发 GetAllExp）

## 目录结构（主要）

- `Assets/Scripts/Game`：角色、敌人、武器、掉落等战斗逻辑
- `Assets/Scripts/System`：升级系统、永久升级、成就、存档等系统模块
- `Assets/Scripts/UI`：UI 面板（开始界面、游戏内 HUD、升级面板、宝箱面板等）
- `Assets/Scenes`：主场景与测试场景
- `Assets/Art`：美术资源、动画、音效、Prefab、图集等
- `Assets/QFramework`：QFramework 源码与工具集
- `Docs/Project-Structure.md`：项目结构整理规范与巡检方式

## 说明

- 本仓库为学习/练习性质的原型工程。
- 课程链接仅作为学习参考；README 为基于本项目代码与提交历史的梳理说明。
