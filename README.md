# Hopper Brain - Automate Rebalance

**Only hoppers activate [Automate](https://www.nexusmods.com/stardewvalley/mods/1063).** Chests become passive storage that a hopper reaches into.

Automate is great, but it makes the early game too easy: any chest placed next to machines feeds them and collects their output, and chests exist on day 1. Hopper Brain keeps everything Automate does (machines, tapped trees, fruit trees, bee houses, crystalariums, shipping bin, Junimo chests, the overlay) and changes one thing: the **hopper is the brain**. Since the vanilla hopper is a late-game item, automation becomes a late-game reward.

## Contents

- [How it works](#how-it-works)
- [Examples](#examples)
- [Install](#install)
- [Getting hoppers](#getting-hoppers)
- [Compatibility](#compatibility)
- [Known limitations](#known-limitations)
- [FAQ](#faq)
- [Building from source](#building-from-source)
- [Credits](#credits)
- [Changelog](#changelog)
- [License](#license)

## How it works

Automate builds "machine groups" out of things that touch each other. Hopper Brain changes who is allowed to touch whom:

| Touching pair | Connects? |
|---|---|
| Hopper – machine | yes |
| Hopper – chest | yes |
| Hopper – hopper | yes |
| Machine – machine | yes (chains, like Automate) |
| Connector – machine / hopper / connector | yes |
| **Chest – machine** | **no** |
| **Chest – chest** | **no** |
| **Chest – connector** | **no** |

"Touching" means any of the 8 surrounding tiles, diagonals included, same as Automate. Multi-tile machines and buildings touch on any of their tiles.

In plain words:

- A **hopper** connects to everything around it.
- **Machines** next to machines form a chain, so one hopper can run a whole row of kegs.
- A **chest** is only used if it **touches a hopper**. A chest next to a machine does nothing.
- **Hoppers link** to each other directly, or through a chest that touches both. That's how you attach more chests.
- A hopper **works without a chest**: it takes inputs from itself and stores outputs in itself. With chests attached, outputs go to the chests first and the hopper only fills when they're full.
- Automate **connectors** (paths or flooring, if you enabled them in Automate's config) act like machines: they extend machine chains but never pull a chest in.

Press **U** (Automate's overlay) to see which groups are active (green) or inactive (red).

## Examples

`B` chest · `H` hopper · `M` machine

```
B M M          nothing works (no hopper)
B H M M        works: chest feeds both machines and receives their output
B M H          the hopper runs the machine alone; the chest is not connected
B B H M        only the chest touching the hopper is used
H M M          works: inputs and outputs live in the hopper
B H [tapped tree]   tree output goes into the chest
```

One hopper, three chests (corners touch diagonally), five machines:

```
B B B
M H M
M M M
```

Two hoppers sharing the middle row of chests, so all 8 chests feed all machines:

```
B B B
B H B
B B B
M H M
M M M
```

## Install

1. Install the latest [SMAPI](https://smapi.io).
2. Install [Automate](https://www.nexusmods.com/stardewvalley/mods/1063) 2.6.1 or later.
3. Download `HopperBrain 1.0.0.zip` from the [releases page](https://github.com/williansaez/HopperBrain/releases) and unzip it into `Stardew Valley/Mods`.
4. Run the game using SMAPI.

You can add or remove the mod mid-save. Nothing is stored in your save: removing it just brings back normal Automate behaviour. Items already inside hoppers stay there.

## Getting hoppers

The hopper's recipe, price and unlock are **unchanged** (vanilla: Mr. Qi's shop on Ginger Island; 10 hardwood, 1 iridium bar, 1 radioactive bar). That's on purpose: Stardew still has plenty of game left after the island.

Want automation earlier? Install [Primitive Automate](https://www.nexusmods.com/stardewvalley/mods/29973) alongside. It makes the hopper cheaper and unlocks it earlier, and its options pair well with this mod.

## Compatibility

- Stardew Valley 1.6.15, SMAPI 4.5.2, Automate 2.6.1: tested in single-player.
- Multiplayer: not tested. Automate only runs automation on the host, and this mod changes the same code, so it should behave the same.
- [Super Hopper](https://www.nexusmods.com/stardewvalley/mods/9418): Automate already ignores super hoppers, so they keep working as before and don't count as a "brain".
- Mods that add machines through Automate (PFMAutomate, Extra Machine Config, etc.): their machines are still machines and follow the rules above.

## Known limitations

- **A connector under a chest doesn't conduct.** If a path or floor tile used as an Automate connector has a chest on top of it, that tile won't link the chain. Not tested in game.
- **Automate updates.** Hopper Brain patches Automate's internals. If an Automate update breaks the patch, Hopper Brain **turns Automate off** and logs an error in the SMAPI console, so plain chests never start automating behind your back. Re-enabling Automate in its config menu while the patch is broken brings chest automation back until the next restart.
- No warning yet when Automate is a version other than 2.6.x.
- Very large connector networks may cause a short hitch when Automate rebuilds its groups (after placing or removing something nearby).

## FAQ

**My machines stopped working after installing this.** That's the point: add a hopper between your chest and your machines. Press **U** to check which groups are green.

**Can I still use regular chests?** Yes, as storage attached to a hopper. A chest only does nothing when no hopper touches it.

**Why does output go into my chest and not the hopper?** Chests attached to a hopper are filled first. The hopper stores output only when it has no chest or the chests are full.

**Does the vanilla hopper behaviour still work?** Yes, the mod doesn't touch the game's own hopper logic.

**Is there a config?** No. The rules are the mod.

## Building from source

Requires the .NET SDK (6.0 or newer) and the game with SMAPI and Automate installed (the build references `Mods/Automate/Automate.dll`).

```bash
git clone https://github.com/williansaez/HopperBrain.git
cd HopperBrain
dotnet test                                  # rule tests, no game needed
dotnet build HopperBrain -c Release          # builds, installs to Mods/, and creates the release zip
```

The release zip lands in `HopperBrain/bin/Release/net6.0/`. Design and plan docs are in [`docs/`](docs/).

Layout:

- `HopperBrain/GroupPartitioner.cs`: the connection rules, pure code with no game references.
- `HopperBrain/AutomatePatches.cs`: Harmony patch that re-splits Automate's groups with those rules.
- `HopperBrain/ModEntry.cs`: setup, lets the hopper store items, and the safety switch that turns Automate off if the patch fails.
- `HopperBrain.Tests/`: xUnit tests for the rules.

## Credits

- [Pathoschild](https://github.com/Pathoschild/StardewMods) for Automate, which does all the real work.
- Inspired by [Super Hopper](https://www.nexusmods.com/stardewvalley/mods/9418), [Hopper Extractor](https://www.nexusmods.com/stardewvalley/mods/16688), [Primitive Automate](https://www.nexusmods.com/stardewvalley/mods/29973), [Smart Filtered Hopper](https://www.nexusmods.com/stardewvalley/mods/44714) and [Hopper Collector](https://www.nexusmods.com/stardewvalley/mods/45783).

## Changelog

### 1.0.0
- First release.

## License

[MIT](LICENSE).
