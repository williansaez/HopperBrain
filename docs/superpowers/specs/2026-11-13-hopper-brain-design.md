# Hopper Brain - Automate Rebalance — Design

Date: 2026-11-13
Status: awaiting user approval

## Intent

Automate (Pathoschild) makes the early game too easy: any chest placed next to machines feeds them and collects their output, and chests are available on day 1. Hopper Brain keeps everything Automate does but makes the **Hopper the brain**: only a hopper activates automation. Chests become passive storage that a hopper reaches into. Since the vanilla hopper is a late-game item (Qi's Gem Shop, Ginger Island; recipe 10 hardwood + 1 iridium bar + 1 radioactive bar), automation moves to the late game.

## Goals

- A hopper connects to everything touching it (8 neighbours, diagonals included, same as Automate).
- Machines chain to machines exactly like Automate (machine next to machine extends the group).
- A chest joins a group **only if it touches a hopper**.
- A hopper with no chest still works: it pulls inputs from and stores outputs in its own inventory.
- Everything Automate supports keeps working through the hopper: tappers/trees, fruit trees, bee houses, crystalariums, shipping bin, etc.
- Several hoppers can be linked through chests or directly, so one setup can use many chests.

## Connection rules (the core)

Tile types: `H` hopper, `B` chest (any non-hopper container), `M` machine (anything Automate treats as a machine, including trees/tappers), connectors per Automate config.

| Neighbour pair | Connects? |
|---|---|
| H – M | yes |
| H – B | yes |
| H – H | yes |
| M – M | yes (Automate chain) |
| B – M | **no** |
| B – B | **no** |
| C – M, C – H, C – C | yes (connector acts like a machine) |
| C – B | **no** |

`C` = Automate connector (paths, flooring, etc. per Automate config). Decided by the user: a connector counts as a **machine**. It only extends machine chains; a chest touching a connector does not join. `H-path-path-M` works; `B-path-M` does nothing.

A group is active only if it contains at least one hopper. Containers in the group = all hoppers + all chests touching a hopper.

### Examples

```
B-M-M        nothing works
B-H-M-M      works; chest feeds and receives from both machines
H-tree       works; output stored in hopper
B-H-tree     works; output stored in chest
B-M-H        hopper automates the machine alone; chest is not connected
B-B-H-M      only the chest touching H is storage; the far chest is out
```

Single hopper, 3x3:

```
B B B
M H M
M M M
```
3 chests (corners via diagonal) are storage; 5 machines automated; machines chained to these extend the group.

Hoppers linked through chests (user's example):

```
B B B
B H B
B B B
M H M
M M M
```
Top hopper reaches its 8 chests. Row-3 chests also touch the bottom hopper, so both hoppers and all 8 chests and all machines form one group.

## Non-goals

- No change to hopper recipe, cost, or unlock (stays vanilla). Users who want earlier hoppers can install Primitive Automate alongside.
- No new items, sprites, or UI. Automate's overlay (U key) is reused as-is.
- No re-implementation of machine logic.

## Anti-goals

- Must not make a plain chest automate anything, in any layout.
- Must not break or require changes to Automate's files on disk.
- Must not silently lose items (hopper/chest contents) when the mod is added or removed.

## Constraints

- Stardew Valley 1.6.15, SMAPI 4.5.2, Automate 2.6.1 (installed on the user's machine). Windows, .NET SDK 10 available; mod targets what SMAPI 4.x requires (net6.0).
- In 1.6 the hopper is a `Chest` with `SpecialChestType.AutoLoader`. Vanilla hopper behaviour (auto-loading the machine to its left) should be left alone unless it conflicts.
- Must work in multiplayer the same way Automate does (host runs automation).

## Approaches considered

- **A. Small SMAPI mod on top of Automate, Harmony patches** — chosen. Depends on Automate; patches how Automate builds machine groups so the connection rules above apply. Keeps all Automate features and updates. Trade-off: patches Automate internals, can break when Automate updates; fix = adjust the patch. Estimate 4–6 h to working in game.
- **B. Fork Automate** — full control, but owning and syncing a whole Automate fork. Rejected.
- **C. From scratch (like Hopper Collector)** — weeks of work re-implementing machine support. Rejected; throws away what the user wants to keep.

## Decided by the user

- Hopper is the brain; chests only via hopper; hopper works without a chest.
- Whole machine chain is automated (not just touching machines).
- Hopper cost/unlock stays vanilla.
- Chest–chest does not connect; hoppers may link through shared chests.
- Connectors count as machines (see rule table).
- On patch failure, automation stops entirely with a logged error.
- Display name **Hopper Brain - Automate Rebalance** (manifest `Name`), UniqueID `WillianSaez.HopperBrain`, folder `C:\Users\williansaez\GitHub\HopperBrain\`.

## Left to the builder

- Find where Automate builds groups (flood fill over tiles / group factory) and the least intrusive patch point for the rule table. Prefer patching the "are these two tiles connected" step over rewriting the fill.
- Confirm whether Automate already treats a hopper (AutoLoader chest) as a container; if it excludes it, include it.
- Decide how to identify Automate types (reflection by type name).
- Failure mode (decided by the user): if any patch target is not found or a patch fails, **stop all Automate automation** (needs a second, simpler hook that disables machine groups) and log a clear error in the SMAPI console. Never fall back to vanilla Automate behaviour, since that would let plain chests automate again.
- Verify in game with the layouts in the Examples section.
