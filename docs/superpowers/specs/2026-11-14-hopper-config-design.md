# Hopper Brain — Configurable hopper cost & unlock — Design

Date: 2026-11-14
Status: awaiting user approval
Builds on: `2026-11-13-hopper-brain-design.md` (v1.0.0, released)

## Intent

v1.0.0 keeps the hopper vanilla (Qi's Walnut Room on Ginger Island), so automation is end-game only. Players who want automation earlier currently need Primitive Automate as a second mod. This adds the same flexibility inside Hopper Brain: choose the hopper recipe cost and when the recipe unlocks. Defaults stay vanilla, so nothing changes for existing players.

## Options (decided by the user)

### Recipe cost (`RecipeCost`, default `Vanilla`)

| Value | Ingredients |
|---|---|
| `Copper` | 50 Wood + 5 Copper Bar |
| `Iron` | 10 Hardwood + 5 Iron Bar |
| `Gold` | 10 Hardwood + 5 Gold Bar + 3 Clay |
| `Iridium` | 10 Hardwood + 1 Iridium Bar + 1 Battery Pack |
| `Vanilla` | 10 Hardwood + 1 Iridium Bar + 1 Radioactive Bar (game default; recipe not edited) |

### Recipe unlock (`RecipeUnlock`, default `Vanilla`)

| Value | Behaviour |
|---|---|
| `Start` | Recipe known from the start |
| `Farming4` | Learned at Farming level 4 |
| `Farming9` | Learned at Farming level 9 |
| `Robin` | Robin's carpentry shop sells the recipe and the hopper |
| `Vanilla` | Game default (Qi's Walnut Room); nothing added |

### Robin prices (only used when `RecipeUnlock = Robin`)

| Option | Default |
|---|---|
| `RobinRecipePrice` | 50000 g |
| `RobinHopperPrice` | 10000 g |

With any other unlock, Robin sells nothing hopper-related.

### Qi's Walnut Room

Unchanged in every mode: still sells the hopper (10 Qi Gems) and the recipe (50 Qi Gems).

## Behaviour

- Changing options mid-save works. If the player already meets the new unlock (e.g. Farming ≥ 4), the recipe is granted the next morning.
- Recipe cost changes apply on the next asset reload (immediately after saving the config in GMCM; otherwise next game launch).
- Switching unlock back to a stricter one does **not** remove an already-learned recipe (never take things away from a save).
- Config UI through Generic Mod Config Menu when installed (optional dependency). Without it, `config.json` is editable.
- Mod page and README get a note: if you use Primitive Automate, leave these options on `Vanilla`.

## Non-goals

- Free-form ingredients or item IDs (presets only).
- Removing the hopper or recipe from Qi's shop.
- Changing the connection rules from v1.0.0.

## Anti-goals

- Defaults must leave the game exactly as v1.0.0 does.
- Never remove a recipe or item the player already has.

## Approaches

- **A. All in C# inside Hopper Brain** — chosen by the user. Edit `Data/CraftingRecipes` (cost), grant the recipe on `DayStarted` (Start/Farming), add entries to `Data/Shops` → `Carpenter` (Robin). One mod, no new required dependency.
- **B. Separate Content Patcher pack** — rejected: extra mod + Content Patcher dependency, options split across two places.

## Builder notes (my calls, user can override)

- Recipe key in `Data/CraftingRecipes` is `Hopper`; item `(BC)275`. Object IDs: Wood 388, Hardwood 709, Copper Bar 334, Iron Bar 335, Gold Bar 336, Iridium Bar 337, Radioactive Bar 910, Clay 330, Battery Pack 787. Verify against the game data at build time.
- Robin shop entries use `Data/Shops` key `Carpenter`; the recipe entry is `(BC)275` with `IsRecipe: true`; hide the recipe entry once learned.
- In multiplayer, unlock checks run per player on their own `DayStarted`.
- Version bump to 1.1.0; README/Nexus description gain a "Configuration" section.
