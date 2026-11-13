# Hopper Brain - Automate Rebalance

Automate makes the early game too easy: any chest next to machines runs them from day 1. Hopper Brain keeps everything Automate does, but **only hoppers activate automation**. Since the hopper is a late-game item, automation moves to the late game.

## Rules

- A **hopper** connects to everything touching it, diagonals included.
- **Machines** touching machines form a chain, same as Automate.
- A **chest** joins a group **only if it touches a hopper**. A chest next to a machine does nothing.
- Chests don't connect to other chests. Hoppers can link to each other directly or through a chest touching both.
- A hopper works without a chest: it takes inputs from and stores outputs in itself. With a chest attached, outputs go to the chest first.
- Automate **connectors** (paths, flooring) act like machines: they extend machine chains but never pull a chest in.

```
B M M          nothing works
B H M M        works
B B B
M H M          works: 3 chests, 5 machines
M M M
```

## Install

1. Install [SMAPI](https://smapi.io) and [Automate](https://www.nexusmods.com/stardewvalley/mods/1063) 2.6.1+.
2. Unzip into `Stardew Valley/Mods`.
3. Run the game using SMAPI.

The hopper's recipe, price and unlock are unchanged. Want hoppers earlier? Use [Primitive Automate](https://www.nexusmods.com/stardewvalley/mods/29973) alongside.

## Known limitations

- **A connector under a chest doesn't conduct.** If a path/floor tile used as an Automate connector has a chest placed on top of it, that tile won't link the chain. Not tested in game.
- Hopper Brain patches Automate's internals. If an Automate update breaks the patch, Hopper Brain **turns Automate off** and logs an error, so plain chests never automate. Re-enabling Automate in its config menu while the patch is broken brings chest automation back until the next restart.
- No warning yet when Automate is a version other than 2.6.x.
- Very large connector networks may cause a short hitch when Automate rebuilds groups.

## Compatibility

Tested with Stardew Valley 1.6.15, SMAPI 4.5.2, Automate 2.6.1, single-player. Super Hopper hoppers are ignored by Automate and keep working as before.
