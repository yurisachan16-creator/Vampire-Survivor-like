# 波次系统（现状）配置表与调用链

## 配置数据源

- 主配置：`Assets/StreamingAssets/Config/EnemyWaveConfig.csv`
- 解析：`EnemyWaveConfigLoader.ParseCSV()` → `EnemyWaveConfigRow`
- 转换：`EnemyWave.FromConfigRow(row)`（定义在 `LevelConfig.cs`）
- 生成：`EnemyGenerator.Start()` → `LoadFromCSVAsync()` → `EnemyGenerator.Update()`

## CSV 列定义（现状）

| 列名 | 含义 | 旧列兼容 |
|---|---|---|
| GroupName | 波次组名（用于把多行配置归为同一“波次组”） | 必填 |
| GroupDescription | 波次组描述 | 可空 |
| WaveName | 当前行的显示名 | 必填 |
| Active | 是否启用 | 必填 |
| EnemyPrefabName | 预制体 Key（由 EnemyPrefabMapping 映射到 Prefab） | 必填 |
| GenerateDuration | 刷新间隔（秒）：每隔多少秒生成 1 个敌人 | 必填 |
| KeepSeconds | 刷新持续时间（秒）：持续生成多久，超过后不再生成新敌人 | 必填 |
| HPScale / SpeedScale / DamageScale | 数值倍率 | 必填 |
| MaxWaitAfterSpawnSeconds | 刷新结束后最大等待时间（秒） | 新列（缺省按默认值） |
| BaseSpeed | 敌人基础速度 | 旧列 |
| IsTreasureChest | 是否掉宝箱（仅 Boss/特定怪有意义） | 旧列 |
| ExpDropRate / CoinDropRate / HpDropRate / BombDropRate | 掉落概率 | 旧列 |

## “波次组”与“行配置”的关系

- **一个 GroupName 表示一个“波次组”**（例如“第一波幽灵”），组内可能有多行配置，代表不同怪物种类/段落。
- 旧实现的生成逻辑是“按行逐个入队”，组内的每一行都被当成独立波次处理，因此会出现：小怪先刷完但 Boss 行还在后面（间隔很大）→ 玩家清空场景后仍要等待 Boss 行开始，体验上像“切波失效”。  

## 生成脚本调用链（现状）

```mermaid
flowchart TD
    A[Game.unity 场景启动] --> B[EnemyGenerator.Start()]
    B --> C{UseCSVConfig?}
    C -- yes --> D[LoadFromCSVAsync()]
    D --> E[EnemyWaveConfigLoader.LoadAsync()]
    E --> F[ParseCSV -> EnemyWaveConfigRow 列表]
    F --> G[EnemyWave.FromConfigRow(row)]
    G --> H[Enqueue 到 _mEnemyWaveQueue]
    C -- no --> I[LoadFromScriptableObject()]
    B --> J[Update()]
    J --> K{_mCurrentWave == null?}
    K -- yes --> L[Dequeue 作为当前行波次]
    L --> M[按 GenerateDuration 刷新]
    M --> N[达到 KeepSeconds 停止刷新]
    N --> O[按清怪/超时切波]
```

## 关键切波点（现状）

- 刷新阶段：持续到 `KeepSeconds` 或被“提前切波”逻辑打断。
- 提前切波（当前已增强）：当场景内 `Enemy` 与 `EnemyMiniBoss` 均为 0 时，会触发切波。
- 刷新结束后的最大等待：`MaxWaitAfterSpawnSeconds` 用于限制“刷新结束后仍未清怪”的拖波时间上限。

