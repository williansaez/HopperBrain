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
| 1 | `B M M` | red, furnaces idle | |
| 2 | `B H M M` | green, both furnaces smelt, bars go to chest | |
| 3 | `B M H` (ore in chest) | chest not used; furnace only uses what's in the hopper | |
| 4 | `B B H M` | only the chest touching H is used | |
| 5 | 3x3: `BBB / MHM / MMM` | green, all 5 furnaces fed from the 3 chests | |
| 6 | `H M M` (ore in hopper) | green, bars stored in the hopper | |
| 7 | `H` touching a tapper on a tree | tapper output goes into the hopper | |
| 8 | `B H` + tapper tree, try chest on each side of the hopper (left, right, above, below) | tapper output goes into the chest in all 4 cases, never the hopper | |
