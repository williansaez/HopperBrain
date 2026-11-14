# In-game verification — Hopper Brain 1.0.0

Setup: load a save, open the SMAPI console and run:

```
player_add (BC)275 10
player_add (BC)130 10
player_add (BC)13 10
player_add (O)378 99
player_add (O)382 50
player_add (BC)105 3
```

(hopper, chest, furnace, copper ore, coal, tapper)

Press **U** to see Automate's overlay: green = active, red = inactive.
After 1 in-game hour, run `automate summary` in the SMAPI console.

`B` chest, `H` hopper, `M` furnace. Put ore + coal in the chest (or hopper when there is no chest).

| # | Layout | Expected | Result |
|---|---|---|---|
| 1 | `B M M` | red, furnaces idle | OK |
| 2 | `B H M M` | green, both furnaces smelt, bars go to chest | OK |
| 3 | `B M H` (ore in chest) | chest not used; furnace only uses what's in the hopper | OK |
| 4 | `B B H M` | only the chest touching H is used | OK |
| 5 | 3x3: `BBB / MHM / MMM` | green, all 5 furnaces fed from the 3 chests | OK |
| 6 | `H M M` (ore in hopper) | green, bars stored in the hopper | OK |
| 7 | `H` touching a tapper on a tree | tapper output goes into the hopper | OK |
| 8 | `B H` + tapper tree, try chest on each side of the hopper (left, right, above, below) | tapper output goes into the chest in all 4 cases, never the hopper | OK |

Result (2026-11-13): user confirmed all layouts work as expected. SMAPI log after the session: save loaded, no errors from Hopper Brain or Automate.

# In-game verification — Hopper Brain 1.1.0 (config)

Change options in the game: title screen or in-game menu → **Mod Options** (Generic Mod Config Menu) → Hopper Brain. Sleep once after changing the unlock.

| # | Setup | Expected | Result |
|---|---|---|---|
| 1 | Defaults (Vanilla / Vanilla), save without the hopper recipe | no hopper recipe in crafting menu; Robin sells nothing new | |
| 2 | Cost `Iron`, unlock `From the start`, sleep | recipe learned; shows 10 Hardwood + 5 Iron Bar | |
| 3 | Cost `Copper`, unlock `Farming level 4`, save with Farming ≥ 4, sleep | recipe learned; shows 50 Wood + 5 Copper Bar | |
| 4 | Unlock `Buy from Robin` (save **without** the recipe) | Robin sells the recipe for 50,000g and a hopper for 10,000g. Note: "[STF] Robin Shop Overhaul" is installed and may replace her stock — if nothing shows, say so | |
| 5 | Buy the recipe from Robin, reopen her shop | recipe gone from her shop; hopper still sold | |
| 6 | Change cost in Mod Options mid-game, open crafting menu | new cost shown without restarting | |
