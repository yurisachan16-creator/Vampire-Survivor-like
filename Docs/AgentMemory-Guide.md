# Agent Memory Guide

## Goal
- Provide a local, shared memory layer for multiple AI agents in this Unity project.
- Use file-based storage only (no HTTP server, no MCP runtime service).
- Support mixed memory types:
  - Key-value memory
  - Knowledge entries with tags and search

## Storage Layout
- Root directory: `Assets/StreamingAssets/Memory`
- Files:
  - `memory_kv.json`
  - `memory_knowledge.jsonl`
  - `memory_index.json`
  - `memory.lock`

## Data Model
Each `MemoryEntry` includes:
- `id`
- `type` (`KeyValue` / `Knowledge`)
- `project`, `agent`, `channel`
- `key`, `value`
- `title`, `content`
- `tags` (normalized to lowercase)
- `sourceAgent`
- `importance` (`0..1`)
- `createdAtUtc`, `updatedAtUtc`, `expiresAtUtc`
- `deleted`

## API
Entry points are in `AgentMemory`:
- `Set(...)`
- `TryGet(...)`
- `GetOrDefault(...)`
- `AddKnowledge(...)`
- `Search(...)`
- `DeleteById(...)`
- `PruneExpired(...)`
- `ExportSnapshot(...)`

You can swap storage implementation in tests/tools via:
- `AgentMemory.ConfigureStore(IAgentMemoryStore store)`
- `AgentMemory.ResetDefaultStore()`

## Search Behavior
- Case-insensitive keyword matching for title/content/key/value/source/tags.
- Tag filters require exact tag matches.
- Scope filter supports partial matching:
  - If query scope field is empty, it is treated as wildcard.
- Sorting:
  - Higher score first
  - If score ties, newer `updatedAtUtc` first
- Default `limit` is `20` when input limit is invalid.

## Expiration and Delete
- Expired entries are hidden by default.
- `PruneExpired(nowUtc)` physically removes expired entries.
- `DeleteById(id)` performs soft delete (`deleted=true`).

## Corruption Recovery
- If JSON / JSONL parsing fails:
  - The bad file is renamed to `*.corrupt.{timestamp}`
  - Memory store rebuilds from an empty state for that file

## Multi-Agent Naming Conventions
Recommended scope naming:
- `project`: fixed project key, e.g. `vampire-survivor-like`
- `agent`: tool or role, e.g. `codex`, `planner`, `reviewer`
- `channel`: usage channel, e.g. `feature`, `qa`, `release`

Example:
```csharp
var scope = new MemoryScope("vampire-survivor-like", "codex", "feature");
AgentMemory.Set("current_task", "wave tuning", scope, "codex");
```

## Running EditMode Tests
Before running tests, close any Unity Editor instance for this project.

```powershell
"C:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Unity.exe" `
  -batchmode -nographics -quit `
  -projectPath "D:\unity\Vampire Survivor-like" `
  -runTests -testPlatform EditMode `
  -testResults "D:\unity\Vampire Survivor-like\TestResults_AgentMemory_EditMode.xml" `
  -logFile "D:\unity\Vampire Survivor-like\UnityTest_AgentMemory_Edit.log"
```
