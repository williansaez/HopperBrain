# Hopper Brain 1.1.0 — Configurable hopper cost & unlock — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let players pick the hopper recipe cost and how the recipe unlocks (incl. buying from Robin), defaulting to vanilla.

**Architecture:** A `ModConfig` holds four options. Pure helpers (`HopperRecipe`) map options → recipe ingredient string and "should the player know the recipe" so they're unit-testable. `ModEntry` applies them: edits `Data/CraftingRecipes` and `Data/Shops`, grants the recipe on `DayStarted`, and registers an optional Generic Mod Config Menu page.

**Tech Stack:** existing project (C#/net6.0, SMAPI 4.5.2, ModBuildConfig 4.4.0, xUnit tests on net10.0).

**Spec:** `docs/superpowers/specs/2026-11-14-hopper-config-design.md`

## Global Constraints

- Defaults: `RecipeCost = Vanilla`, `RecipeUnlock = Vanilla`, `RobinRecipePrice = 50000`, `RobinHopperPrice = 10000`. Defaults must leave the game exactly as v1.0.0.
- Qi's Walnut Room is never edited.
- Never remove a recipe or item the player already has.
- Robin sells nothing hopper-related unless `RecipeUnlock = Robin`.
- Recipe key `Hopper`; item `(BC)275`; shop key `Carpenter`.
- Ingredient object IDs: Wood 388, Hardwood 709, Copper Bar 334, Iron Bar 335, Gold Bar 336, Iridium Bar 337, Radioactive Bar 910, Clay 330, Battery Pack 787. Verify in game (Task 3).
- GMCM is an optional dependency (`spacechase0.GenericModConfigMenu`, `IsRequired: false`).
- Version `1.1.0` in csproj and manifest.

## Review Focus

1. **Cost changed mid-session via GMCM** → recipe must update without restart: invalidate `Data/CraftingRecipes` and `Data/Shops` on GMCM save. (Task 2, verified in Task 3.)
2. **Recipe already known, then unlock switched to Robin** → Robin must not sell the recipe again: shop entry `Condition` `!PLAYER_HAS_CRAFTING_RECIPE Current Hopper`. (Task 2, Task 3 check.)
3. **Farming level reached mid-day** → recipe granted next morning (DayStarted), not instantly; acceptable per spec. (Test `ShouldKnow_Farming4_AtLevel4`.)
4. **`Vanilla` cost** → recipe entry must not be touched at all (other mods may edit it). (Test `Ingredients_Vanilla_IsNull`.)
5. **Invalid config values** (e.g. negative price, unknown enum in JSON) → clamp price to ≥ 0; SMAPI falls back to defaults for unparseable enums. (Test `ClampPrice_Negative_IsZero`.)

---

### Task 1: Pure recipe options

**Files:**
- Create: `HopperBrain/HopperRecipe.cs`
- Modify: `HopperBrain.Tests/HopperBrain.Tests.csproj` (add `<Compile Include="..\HopperBrain\HopperRecipe.cs" Link="HopperRecipe.cs" />`)
- Test: `HopperBrain.Tests/HopperRecipeTests.cs`

**Interfaces (namespace `HopperBrain`):**
- `public enum RecipeCost { Vanilla, Copper, Iron, Gold, Iridium }`
- `public enum RecipeUnlock { Vanilla, Start, Farming4, Farming9, Robin }`
- `public static class HopperRecipe`
  - `public static string? Ingredients(RecipeCost cost)` — `null` for Vanilla, else the space-separated `id count` string.
  - `public static bool ShouldKnow(RecipeUnlock unlock, int farmingLevel)` — true for `Start`; `Farming4` when level ≥ 4; `Farming9` when level ≥ 9; false for `Vanilla` and `Robin`.
  - `public static int ClampPrice(int price)` — `Math.Max(0, price)`.

- [ ] **Step 1: Write failing tests** in `HopperRecipeTests.cs`:

```csharp
[Fact] Ingredients_Vanilla_IsNull()                         // null
[Theory] Ingredients_MatchSpec(cost, expected)
  // Copper  "388 50 334 5"
  // Iron    "709 10 335 5"
  // Gold    "709 10 336 5 330 3"
  // Iridium "709 10 337 1 787 1"
[Theory] ShouldKnow(unlock, level, expected)
  // Start 0 true; Farming4 3 false; Farming4 4 true; Farming9 8 false; Farming9 9 true; Vanilla 10 false; Robin 10 false
[Fact] ClampPrice_Negative_IsZero()                         // -5 → 0; 50000 → 50000
```

- [ ] **Step 2: Run** `dotnet test` → Expected: build error, `HopperRecipe` not found.
- [ ] **Step 3: Implement** `HopperRecipe.cs` per Interfaces (switch expressions; no game references).
- [ ] **Step 4: Run** `dotnet test` → Expected: all pass (23 existing + new).
- [ ] **Step 5: Commit** `feat: add hopper recipe option helpers`.

---

### Task 2: Config, content edits, unlock, GMCM

**Files:**
- Create: `HopperBrain/ModConfig.cs`, `HopperBrain/IGenericModConfigMenuApi.cs`
- Modify: `HopperBrain/ModEntry.cs`, `HopperBrain/manifest.json`, `HopperBrain/HopperBrain.csproj` (Version 1.1.0)

**Interfaces:**
- Consumes: `RecipeCost`, `RecipeUnlock`, `HopperRecipe.*` (Task 1).
- `internal sealed class ModConfig { RecipeCost RecipeCost = Vanilla; RecipeUnlock RecipeUnlock = Vanilla; int RobinRecipePrice = 50000; int RobinHopperPrice = 10000; }` (auto-properties).
- `IGenericModConfigMenuApi`: copy only the members used — `Register`, `AddSectionTitle`, `AddTextOption` (with `allowedValues` + `formatAllowedValue`), `AddNumberOption` (int overload) — signatures exactly as in GMCM's published API.

- [ ] **Step 1: Config + manifest.** `ModConfig` as above; `ModEntry.Entry` reads it with `helper.ReadConfig<ModConfig>()`. Manifest: `Version 1.1.0`, add optional dependency `spacechase0.GenericModConfigMenu`. Build → Expected: success; `config.json` created in `Mods/HopperBrain` on first launch.

- [ ] **Step 2: Recipe cost edit.** In the existing `OnAssetRequested`, for `Data/CraftingRecipes`: if `HopperRecipe.Ingredients(config.RecipeCost)` is not null, replace field 0 (before the first `/`) of entry `Hopper`, keep the other fields. Do nothing for Vanilla.

- [ ] **Step 3: Robin shop.** For `Data/Shops` when `RecipeUnlock == Robin`: append to `Carpenter`'s `Items` two `ShopItemData`:
  - `Id "WillianSaez.HopperBrain_Recipe"`, `ItemId "(BC)275"`, `IsRecipe true`, `Price ClampPrice(RobinRecipePrice)`, `Condition "!PLAYER_HAS_CRAFTING_RECIPE Current Hopper"`.
  - `Id "WillianSaez.HopperBrain_Hopper"`, `ItemId "(BC)275"`, `Price ClampPrice(RobinHopperPrice)`.

- [ ] **Step 4: Unlock.** On `GameLoop.DayStarted`: if `HopperRecipe.ShouldKnow(config.RecipeUnlock, Game1.player.FarmingLevel)` and `!Game1.player.craftingRecipes.ContainsKey("Hopper")`, add `("Hopper", 0)` and log at `Info` "Learned the hopper recipe ({unlock}).". Never removes.

- [ ] **Step 5: GMCM.** On `GameLoop.GameLaunched`, get the API (null → skip). Register with reset → `new ModConfig()`, save → `WriteConfig` then `Helper.GameContent.InvalidateCache("Data/CraftingRecipes")` and `("Data/Shops")`. Options: section "Hopper recipe"; `RecipeCost` and `RecipeUnlock` as text options with allowed values = enum names; section "Robin's shop (unlock = Robin)"; two number options (min 0). Tooltips mention "Leave on Vanilla if you use Primitive Automate".

- [ ] **Step 6: Build** `dotnet build HopperBrain` → Expected: success, deployed. Run `dotnet test` → all pass.

- [ ] **Step 7: Commit** `feat: configurable hopper recipe cost and unlock`.

---

### Task 3: Verify in game, docs, release

**Files:** `docs/verification.md` (append a 1.1.0 section), `README.md` (Configuration section + Changelog 1.1.0).

- [ ] **Step 1: Agent smoke test.** Launch SMAPI, read log: `Hopper Brain - Automate Rebalance 1.1.0` loads, no errors; `config.json` exists with defaults.
- [ ] **Step 2: User checks** (agent writes the checklist in `docs/verification.md`):
  1. Defaults: crafting menu has no hopper recipe (fresh save), Robin sells nothing new.
  2. `Iron` + `Start`: after sleeping, recipe shows 10 Hardwood + 5 Iron Bar.
  3. `Copper` + `Farming4` on a save with Farming ≥ 4: recipe learned next morning with 50 Wood + 5 Copper Bar.
  4. `Robin`: Robin sells recipe at 50000g and hopper at 10000g; after buying the recipe it disappears from her shop.
  5. Change cost in GMCM mid-session: crafting menu shows the new cost without restarting.
- [ ] **Step 3: Docs.** README: add "Configuration" (table of the 4 options + defaults + Primitive Automate note) and Changelog `1.1.0 — Configurable hopper recipe cost and unlock (Robin shop option)`.
- [ ] **Step 4: Commit + release (after user confirms checks).** Commit `docs: 1.1.0 configuration and verification`, push, tag `v1.1.0`, `gh release create v1.1.0` with the Release zip (`HopperBrain.1.1.0.zip`).
