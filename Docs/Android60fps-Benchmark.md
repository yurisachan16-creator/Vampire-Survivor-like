# Android 60FPS Benchmark Baseline

## Target

- Device tier: Android mid-range (778G / Dimensity 1080 class).
- Build: `Development Build`, `IL2CPP`, Android.
- Resolution: 1080p profile.

## Scenes and duration

1. `Assets/Scenes/Game.unity`: normal gameplay for 20 minutes.
2. `Assets/Scenes/TestMaxEnemyCount.unity`: 5 minutes.
3. `Assets/Scenes/TestMaxPowerUpCount.unity`: 5 minutes.

Use `PerformanceHud` export to generate CSV/JSON for each run.

## Pass criteria

- Avg FPS >= 58.
- P95 frame time <= 18ms.
- Continuous frame spikes >33ms: <= 3 times per 10 minutes.
- P95 `GC Allocated In Frame` <= 1KB.

## Validation checklist

1. Gameplay regression: 10-minute run with all weapons unlocked.
2. Adaptive fallback: force heavy load, verify `Adaptive Perf: Degraded` and post-processing/EXP pulse throttling take effect.
3. Recovery: when load drops, verify adaptive mode returns to `Normal` after sustained FPS recovery.
