# Hopper Brain - Automate Rebalance

**Only hoppers activate [Automate](https://www.nexusmods.com/stardewvalley/mods/1063).** Chests become passive storage that a hopper reaches into.

Automate is great, but it makes the early game too easy: any chest placed next to machines feeds them and collects their output, and chests exist on day 1. Hopper Brain keeps everything Automate does (machines, tapped trees, fruit trees, bee houses, crystalariums, shipping bin, Junimo chests, the overlay) and changes one thing: the **hopper is the brain**. Since the vanilla hopper is a late-game item, automation becomes a late-game reward.

## Contents

- [How it works](#how-it-works)
- [Examples](#examples)
- [Install](#install)
- [Getting hoppers](#getting-hoppers)
- [Configuration](#configuration)
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
3. Download the latest `HopperBrain.x.y.z.zip` from the [releases page](https://github.com/williansaez/HopperBrain/releases) and unzip it into `Stardew Valley/Mods`.
4. Run the game using SMAPI.

You can add or remove the mod mid-save. Nothing is stored in your save: removing it just brings back normal Automate behaviour. Items already inside hoppers stay there.

## Getting hoppers

By default the hopper's recipe, price and unlock are **vanilla** (Mr. Qi's Walnut Room on Ginger Island; 10 hardwood, 1 iridium bar, 1 radioactive bar). That's on purpose: Stardew still has plenty of game left after the island.

Want automation earlier? Change the recipe cost and unlock in the [configuration](#configuration). Mr. Qi keeps selling the hopper and its recipe in every mode.

## Configuration

Open **Mod Options** with [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) (optional), or edit `Mods/HopperBrain/config.json` after the first launch.

| Option | Values | Default |
|---|---|---|
| `RecipeCost` | `Vanilla` (10 hardwood, 1 iridium bar, 1 radioactive bar) · `Copper` (50 wood, 5 copper bars) · `Iron` (10 hardwood, 5 iron bars) · `Gold` (10 hardwood, 5 gold bars, 3 clay) · `Iridium` (10 hardwood, 1 iridium bar, 1 battery pack) | `Vanilla` |
| `RecipeUnlock` | `Vanilla` (Qi's Walnut Room) · `Start` · `Farming4` · `Farming9` · `Robin` (buy at Robin's shop) | `Vanilla` |
| `RobinRecipePrice` | gold, used only when `RecipeUnlock` is `Robin` | `50000` |
| `RobinHopperPrice` | gold per hopper, used only when `RecipeUnlock` is `Robin` | `10000` |

- A new unlock takes effect the **next morning**. A recipe you already learned is never removed.
- `Farming4` / `Farming9` use your base Farming level, not food or gear buffs.
- Robin sells nothing hopper-related unless `RecipeUnlock` is `Robin`. Her recipe entry disappears once you know the recipe.
- Changing the cost in Mod Options updates the recipe without restarting (reopen the crafting menu).
- If `config.json` has an invalid value, the mod logs a warning and uses the defaults.
- **Using [Primitive Automate](https://www.nexusmods.com/stardewvalley/mods/29973)?** Leave both options on `Vanilla` so the two mods don't fight over the recipe.

## Compatibility

- Stardew Valley 1.6.15, SMAPI 4.5.2, Automate 2.6.1: tested in single-player.
- Multiplayer: not tested. Automate only runs automation on the host, and this mod changes the same code, so it should behave the same. Configuration is per player: each player's own `config.json` decides their recipe cost, unlock and Robin's shop.
- Mods that replace Robin's whole shop (for example Shop Tile Framework packs like Robin Shop Overhaul) may hide the `Robin` unlock entries.
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

**Is there a config?** Yes, for the hopper recipe cost and unlock (see [Configuration](#configuration)). The connection rules themselves aren't configurable: the rules are the mod.

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
- `HopperBrain/HopperRecipe.cs`, `HopperBrain/ModConfig.cs`: recipe cost/unlock options, pure code.
- `HopperBrain/ModEntry.cs`: setup, lets the hopper store items, recipe/shop edits, Mod Options page, and the safety switch that turns Automate off if the patch fails.
- `HopperBrain.Tests/`: xUnit tests for the rules and options.

## Credits

- [Pathoschild](https://github.com/Pathoschild/StardewMods) for Automate, which does all the real work.
- Inspired by [Super Hopper](https://www.nexusmods.com/stardewvalley/mods/9418), [Hopper Extractor](https://www.nexusmods.com/stardewvalley/mods/16688), [Primitive Automate](https://www.nexusmods.com/stardewvalley/mods/29973), [Smart Filtered Hopper](https://www.nexusmods.com/stardewvalley/mods/44714) and [Hopper Collector](https://www.nexusmods.com/stardewvalley/mods/45783).

## Changelog

### 1.1.0
- Configurable hopper recipe cost (Copper, Iron, Gold, Iridium, Vanilla) and unlock (start, Farming 4, Farming 9, buy from Robin, Vanilla). Defaults stay vanilla.
- Optional Generic Mod Config Menu page.
- Invalid `config.json` values fall back to defaults with a warning.

### 1.0.0
- First release.

## License

[MIT](LICENSE).
