# Hopper Brain - Automate Rebalance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A SMAPI mod that makes the Hopper the only thing that activates Automate: chests join a machine group only by touching a hopper.

**Architecture:** Automate builds machine groups in `MachineGroupFactory.GetMachineGroups` with a flood fill where anything touches anything. Our connection rules only remove edges, so every group we want is a subset of an Automate group. A Harmony postfix on that method takes each Automate group, re-partitions it with our rule table (pure, unit-tested `GroupPartitioner`), and rebuilds each part as a new `MachineGroup`. Parts without a hopper naturally end up with no container or no machine, so Automate itself marks them inactive (red in the U overlay). A content edit removes Automate's "take-only" tag from the hopper so it can also store output.

**Tech Stack:** C# / net6.0, SMAPI 4.5.2, Pathoschild.Stardew.ModBuildConfig (latest 4.x), Harmony (bundled with SMAPI), Krafs.Publicizer (compile-time access to Automate internals), xUnit for the pure partitioner.

**Spec:** `docs/superpowers/specs/2026-11-13-hopper-brain-design.md`

## Global Constraints

- Stardew Valley 1.6.15, SMAPI 4.5.2, Automate 2.6.1 (`Pathoschild.Automate`, installed at `$MODS\Automate`).
- Display name `Hopper Brain - Automate Rebalance`; UniqueID `WillianSaez.HopperBrain`; author `WillianSaez`.
- Manifest dependency: `Pathoschild.Automate`, `MinimumVersion: 2.6.1`, required.
- Target `net6.0` (built with the installed .NET SDK 10).
- Hopper = container whose `TypeId == "(BC)275"`. Automate already excludes Super Hopper chests (`modData["spacechase0.SuperHopper"]`); leave that alone.
- Adjacency = 8 neighbours incl. diagonals, on tile areas (multi-tile entities), same as Automate's `Rectangle.GetSurroundingTiles()`.
- Connection rule table (spec): H–M, H–B, H–H, M–M, C–M, C–H, C–C connect; B–M, B–B, C–B do not.
- On any patch failure: stop all Automate automation and log an error. Never fall back to vanilla Automate behaviour.
- No change to hopper recipe, price or unlock. No new items, sprites or UI.
- Do not modify Automate files on disk.

## Review Focus

1. **Multi-tile entities** (shipping bin building, Junimo hut, 2-tile-tall machines): adjacency must use the whole tile area, not the origin tile. → `GroupPartitionerTests.MultiTileMachine_TouchingHopperOnAnyTile_Connects` (Task 1).
2. **Two entities on the same tile** (indoor pot + bush, connector under a machine): overlapping areas count as touching. → `GroupPartitionerTests.OverlappingAreas_Connect` (Task 1).
3. **Hopper with machines and no chest**: must be one active group (hopper is the container). → `GroupPartitionerTests.HopperAloneWithMachines_OneGroup` (Task 1).
4. **Connector bridging a chest to a hopper** (`B-C-H`): chest must stay out. → `GroupPartitionerTests.ConnectorDoesNotLinkChest` (Task 1).
5. **Automate update changes internals**: patch failure must disable automation, not silently restore chest automation. → Task 2 Step 5 (forced failure, check log + Automate disabled).

---

## File Structure

```
HopperBrain/
  .gitignore                         bin/ obj/ external/ *.user
  HopperBrain.sln
  HopperBrain/
    HopperBrain.csproj               mod project (ModBuildConfig, Harmony, Publicizer, ref Automate.dll)
    manifest.json
    ModEntry.cs                      entry: asset edit + Harmony patch + fail-safe
    GroupPartitioner.cs              pure rule table + union-find, no game types
    AutomatePatches.cs               Harmony postfix that adapts Automate groups ↔ partitioner
  HopperBrain.Tests/
    HopperBrain.Tests.csproj         xUnit, links ../HopperBrain/GroupPartitioner.cs only
    GroupPartitionerTests.cs
  external/StardewMods/              Automate source, reference only (gitignored)
  docs/...
```

---

### Task 1: Pure group partitioner

**Files:**
- Create: `HopperBrain/.gitignore`, `HopperBrain/HopperBrain.sln`
- Create: `HopperBrain/HopperBrain/GroupPartitioner.cs`
- Create: `HopperBrain/HopperBrain.Tests/HopperBrain.Tests.csproj` (net6.0, xUnit; `<Compile Include="..\HopperBrain\GroupPartitioner.cs" />`)
- Test: `HopperBrain/HopperBrain.Tests/GroupPartitionerTests.cs`

**Interfaces:**
- Produces (namespace `HopperBrain`):
  - `public enum NodeKind { Hopper, Chest, Machine, Connector }`
  - `public readonly record struct TileArea(int X, int Y, int Width, int Height)`
  - `public readonly record struct Node(NodeKind Kind, TileArea Area)`
  - `public static class GroupPartitioner`
    - `public static bool CanConnect(NodeKind a, NodeKind b)` — the rule table.
    - `public static bool Touches(TileArea a, TileArea b)` — true if `a` expanded by 1 tile on every side intersects `b` (covers overlap and diagonals).
    - `public static List<List<int>> Partition(IReadOnlyList<Node> nodes)` — indices of `nodes` grouped into connected components where an edge exists iff `Touches && CanConnect`. Every index appears in exactly one group. Order of groups and of indices within a group: ascending by first index.

- [ ] **Step 1: `git init` in `HopperBrain/`, add `.gitignore` (`bin/`, `obj/`, `external/`, `*.user`, `.vs/`), create sln + test project, add test project to sln.**

- [ ] **Step 2: Write the failing tests** in `GroupPartitionerTests.cs`. Helper `N(kind, x, y, w=1, h=1)`. Each test asserts the partition as sets of indices:

```csharp
[Fact] void ChestMachine_NoHopper_AllSeparate()            // B-M-M  : {0},{1,2}
[Fact] void ChestHopperMachines_OneGroup()                // B-H-M-M: {0,1,2,3}
[Fact] void ChestMachineHopper_ChestOut()                 // B-M-H  : {0},{1,2}
[Fact] void ChestChestHopperMachine_FarChestOut()         // B-B-H-M: {0},{1,2,3}
[Fact] void Grid3x3_DiagonalChestsJoin()                  // spec 3x3: all 9 one group
[Fact] void TwoHoppersLinkedThroughChests_OneGroup()      // spec 5x3 example: all 15 one group
[Fact] void HoppersDirectlyAdjacent_Connect()             // H-H: {0,1}
[Fact] void ConnectorChain_HopperToMachine_Connects()     // H-C-C-M: one group
[Fact] void ConnectorDoesNotLinkChest()                   // B-C-M and B-C-H: chest alone
[Fact] void HopperAloneWithMachines_OneGroup()            // H-M-M: one group
[Fact] void MultiTileMachine_TouchingHopperOnAnyTile_Connects() // M at (0,0,1,2), H at (1,1): {0,1}
[Fact] void OverlappingAreas_Connect()                    // C and M both at (0,0) next to H: one group
[Fact] void CanConnect_MatchesRuleTable()                 // all 16 ordered pairs vs spec table
```

Layouts are horizontal rows at y=0 unless the name says grid; the 3x3 and 5x3 grids are copied from the spec's Examples section.

- [ ] **Step 3: Run tests, expect compile failure / FAIL.**
Run: `dotnet test HopperBrain.Tests`
Expected: build error "GroupPartitioner does not exist".

- [ ] **Step 4: Implement `GroupPartitioner.cs`** per Interfaces. Union-find over all pairs (groups are small; O(n²) is fine — shortcut comment: `shortcut: O(n²) pair scan, switch to tile index if a group ever exceeds ~2000 entities`). No game/XNA references in this file.

- [ ] **Step 5: Run tests, expect PASS.**
Run: `dotnet test HopperBrain.Tests`
Expected: 13 passed, 0 failed.

- [ ] **Step 6: Commit** `feat: add hopper connection rule partitioner`.

---

### Task 2: Mod project wiring Automate

**Files:**
- Create: `HopperBrain/HopperBrain/HopperBrain.csproj`, `manifest.json`, `ModEntry.cs`, `AutomatePatches.cs`
- Modify: `HopperBrain/HopperBrain.sln` (add project)

**Interfaces:**
- Consumes: `GroupPartitioner.Partition`, `Node`, `NodeKind`, `TileArea` (Task 1).
- Uses Automate internals (namespace `Pathoschild.Stardew.Automate.Framework`, made accessible by Krafs.Publicizer): `MachineGroupFactory.GetMachineGroups(GameLocation, IMonitor) : IEnumerable<IMachineGroup>`, its private fields `BuildStorage : Func<IContainer[], StorageManager>` and `Monitor : IMonitor`, `IMachineGroup.{LocationKey, Machines, Containers, GetTiles(string)}`, `MachineGroup(string? locationKey, IEnumerable<IMachine> machines, IEnumerable<IContainer> containers, IEnumerable<Vector2> tiles, Func<IContainer[], StorageManager> buildStorage, IMonitor monitor)`, `IContainer.TypeId`, `IAutomatable.TileArea`, `AutomateConstants.StorageTakeOnlyTag` (= `"automate_storage_take"`), `ModEntry.Config.Enabled` (private `Config` field).
- Produces: `internal static class AutomatePatches { public static void Apply(Harmony harmony, IMonitor monitor); }` — throws on any failure.

**csproj essentials:** `net6.0`; `PackageReference` `Pathoschild.Stardew.ModBuildConfig` (latest 4.x) with `<EnableHarmony>true</EnableHarmony>`; `Krafs.Publicizer` with `<Publicize Include="Automate" />`; `<Reference Include="Automate" HintPath="$(GameModsPath)\Automate\Automate.dll" Private="false" />`. Exclude `HopperBrain.Tests/**` from compile. ModBuildConfig deploys to `$MODS\HopperBrain` on build.

- [ ] **Step 1: Create csproj + `manifest.json`** (values from Global Constraints; `EntryDll: HopperBrain.dll`, `Version: 1.0.0`, `MinimumApiVersion: 4.5.0`, `Dependencies: [{ UniqueID: "Pathoschild.Automate", MinimumVersion: "2.6.1" }]`). Run `dotnet build HopperBrain` → expected: build succeeds (with an empty `ModEntry : Mod`), mod folder appears in `$MODS\HopperBrain`.

- [ ] **Step 2: Hopper can store.** In `ModEntry.Entry`, subscribe `Content.AssetRequested`; for `Data/BigCraftables` call `e.Edit(..., AssetEditPriority.Late)` and remove `"automate_storage_take"` from entry `"275"`'s `ContextTags`. Late priority is required so it runs after Automate's edit adds the tag.

- [ ] **Step 3: Implement `AutomatePatches`.** Postfix on `MachineGroupFactory.GetMachineGroups` with `ref IEnumerable<IMachineGroup> __result, Func<IContainer[], StorageManager> ___BuildStorage, IMonitor ___Monitor`; replace `__result` with a lazy enumerable that, for each original group:
  1. Builds nodes: each machine → `Machine`; each container → `Hopper` if `TypeId == "(BC)275"` else `Chest`; each tile in `GetTiles(LocationKey)` not covered by any machine/container area → 1×1 `Connector`.
  2. Calls `GroupPartitioner.Partition`.
  3. Yields one `new MachineGroup(LocationKey, machinesInPart, containersInPart, tilesInPart, ___BuildStorage, ___Monitor)` per part, where `tilesInPart` = all tiles of every node in the part. Keep machines in the original group's order (it is already priority-sorted and wrapped).
  If the original group's `LocationKey` is null, yield it unchanged.

- [ ] **Step 4: Fail-safe in `ModEntry.Entry`.** `try { AutomatePatches.Apply(new Harmony(ModManifest.UniqueID), Monitor); } catch (Exception ex) { DisableAutomate(ex); }`. `DisableAutomate`: log at `LogLevel.Error` `"Hopper Brain could not patch Automate (incompatible Automate version?). Automate has been disabled so plain chests do not automate. Details: {ex}"`, then set Automate's `Config.Enabled = false` via `Helper.ModRegistry.Get("Pathoschild.Automate")` → reflect the `Mod` instance (`IModInfo` is SMAPI's `IModMetadata`; read its `Mod` property) → `Helper.Reflection.GetField<object>(mod, "Config")` → set `Enabled`. If that also throws, log a second error telling the player to remove Automate or Hopper Brain. Patch is applied in `Entry`, before Automate builds any groups (groups are built on save load).

- [ ] **Step 5: Verify fail-safe once.** Temporarily throw at the top of `Apply`, build, launch via SMAPI, confirm the error line appears in `SMAPI-latest.txt` and `automate summary` reports automation disabled / no machines automated. Remove the throw. Use Cabana `summarize_log` to read the log.

- [ ] **Step 6: Build and smoke-test load.** `dotnet build HopperBrain`, launch game via SMAPI, load a save. Expected in log: both `Automate 2.6.1` and `Hopper Brain - Automate Rebalance 1.0.0` loaded, no errors from `WillianSaez.HopperBrain`.

- [ ] **Step 7: Commit** `feat: gate Automate groups behind hoppers`.

---

### Task 3: In-game verification (with the user)

**Files:** none (create `HopperBrain/docs/verification.md` with results).

Needs the player in game; the agent prepares the checklist and reads the SMAPI log, the user places objects and reports what the U overlay shows (green = active, red = inactive).

- [ ] **Step 1: Give the user the layouts** from the spec's Examples section plus: `H-tapper tree`, `B-H-tapper tree`, `H-path-path-furnace`, `B-path-furnace`, and the two grids. Expected per layout = spec Examples.
- [ ] **Step 2: For each, user checks overlay and runs `automate summary`** in the SMAPI console after one in-game hour. Expected: groups and machine counts match spec; chest-only layouts show no automated machines; hopper-without-chest layout fills the hopper with output.
- [ ] **Step 3: Record results** in `docs/verification.md`, fix any mismatch via a new failing test in Task 1's suite first.
- [ ] **Step 4: Commit** `docs: record in-game verification`.
