using System;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Shops;

namespace HopperBrain;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod
{
    /// <summary>Automate's context tag which makes a storage take-only.</summary>
    private const string TakeOnlyTag = "automate_storage_take";

    /// <summary>The hopper's key in Data/CraftingRecipes.</summary>
    private const string RecipeName = "Hopper";

    /// <summary>The hopper's qualified item ID.</summary>
    private const string HopperItemId = "(BC)275";

    /// <summary>Whether Automate was already disabled, to avoid repeating the error.</summary>
    private bool Disabled;

    /// <summary>The mod settings.</summary>
    private ModConfig Config = null!; // set in Entry

    public override void Entry(IModHelper helper)
    {
        // a broken config must not stop the Automate patch below
        try
        {
            this.Config = helper.ReadConfig<ModConfig>();
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"Couldn't read config.json, using defaults. Fix or delete the file to change settings. Details: {ex.Message}", LogLevel.Warn);
            this.Config = new ModConfig();
        }
        this.Config.Normalize();

        helper.Events.Content.AssetRequested += this.OnAssetRequested;
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;

        try
        {
            AutomatePatches.Apply(new Harmony(this.ModManifest.UniqueID), this.DisableAutomate);
        }
        catch (Exception ex)
        {
            this.DisableAutomate(ex);
        }
    }

    /// <summary>Edit game data: hopper storage, recipe cost, and Robin's shop.</summary>
    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        // let the hopper store items too (Automate marks it take-only); late priority so this runs after Automate adds the tag
        if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, BigCraftableData>().Data;
                if (data.TryGetValue("275", out BigCraftableData? hopper))
                    hopper.ContextTags?.RemoveAll(tag => tag == TakeOnlyTag);
            }, AssetEditPriority.Late);
        }

        // recipe cost (vanilla leaves the recipe untouched)
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes") && HopperRecipe.Ingredients(this.Config.RecipeCost) is string ingredients)
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, string>().Data;
                if (data.TryGetValue(RecipeName, out string? recipe))
                {
                    int slash = recipe.IndexOf('/');
                    data[RecipeName] = slash < 0 ? ingredients : ingredients + recipe[slash..];
                }
            });
        }

        // Robin sells the recipe and hopper only when unlock = Robin
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops") && this.Config.RecipeUnlock == RecipeUnlock.Robin)
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, ShopData>().Data;
                if (!data.TryGetValue("Carpenter", out ShopData? shop))
                    return;

                shop.Items.Add(new ShopItemData
                {
                    Id = $"{this.ModManifest.UniqueID}_Recipe",
                    ItemId = HopperItemId,
                    IsRecipe = true,
                    Price = HopperRecipe.ClampPrice(this.Config.RobinRecipePrice),
                    Condition = $"!PLAYER_HAS_CRAFTING_RECIPE Current {RecipeName}"
                });
                shop.Items.Add(new ShopItemData
                {
                    Id = $"{this.ModManifest.UniqueID}_Hopper",
                    ItemId = HopperItemId,
                    Price = HopperRecipe.ClampPrice(this.Config.RobinHopperPrice)
                });
            }, AssetEditPriority.Late); // after packs that replace the whole Carpenter shop
        }
    }

    /// <summary>Grant the recipe if the player meets the unlock rule. Never removes it.</summary>
    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        Farmer player = Game1.player;
        if (HopperRecipe.ShouldKnow(this.Config.RecipeUnlock, player.farmingLevel.Value /* base level, ignoring food/gear buffs */) && !player.craftingRecipes.ContainsKey(RecipeName))
        {
            player.craftingRecipes.Add(RecipeName, 0);
            this.Monitor.Log($"Learned the hopper recipe ({this.Config.RecipeUnlock}).", LogLevel.Info);
        }
    }

    /// <summary>Register the config UI with Generic Mod Config Menu, if installed.</summary>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var gmcm = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (gmcm is null)
            return;

        // lambdas so labels follow the game language even if it changes after launch
        string T(string key) => this.Helper.Translation.Get(key);

        gmcm.Register(
            mod: this.ModManifest,
            reset: () => this.Config = new ModConfig(),
            save: () =>
            {
                this.Config.Normalize();
                this.Helper.WriteConfig(this.Config);
                this.Helper.GameContent.InvalidateCache("Data/CraftingRecipes");
                this.Helper.GameContent.InvalidateCache("Data/Shops");
            }
        );

        gmcm.AddSectionTitle(this.ModManifest, () => T("config.section.recipe"));
        gmcm.AddTextOption(
            mod: this.ModManifest,
            name: () => T("config.cost.name"),
            tooltip: () => T("config.cost.tooltip"),
            getValue: () => this.Config.RecipeCost.ToString(),
            setValue: value => this.Config.RecipeCost = Enum.Parse<RecipeCost>(value),
            allowedValues: Enum.GetNames<RecipeCost>(),
            formatAllowedValue: value => T($"config.cost.{value}")
        );
        gmcm.AddTextOption(
            mod: this.ModManifest,
            name: () => T("config.unlock.name"),
            tooltip: () => T("config.unlock.tooltip"),
            getValue: () => this.Config.RecipeUnlock.ToString(),
            setValue: value => this.Config.RecipeUnlock = Enum.Parse<RecipeUnlock>(value),
            allowedValues: Enum.GetNames<RecipeUnlock>(),
            formatAllowedValue: value => T($"config.unlock.{value}")
        );

        gmcm.AddSectionTitle(this.ModManifest, () => T("config.section.robin"), () => T("config.section.robin.tooltip"));
        gmcm.AddNumberOption(
            mod: this.ModManifest,
            name: () => T("config.robin-recipe.name"),
            tooltip: () => T("config.robin-recipe.tooltip"),
            getValue: () => this.Config.RobinRecipePrice,
            setValue: value => this.Config.RobinRecipePrice = value,
            min: 0
        );
        gmcm.AddNumberOption(
            mod: this.ModManifest,
            name: () => T("config.robin-hopper.name"),
            tooltip: () => T("config.robin-hopper.tooltip"),
            getValue: () => this.Config.RobinHopperPrice,
            setValue: value => this.Config.RobinHopperPrice = value,
            min: 0
        );
    }

    /// <summary>Turn Automate off so plain chests can never automate when the patch isn't active.</summary>
    /// <param name="ex">Why the patch failed.</param>
    private void DisableAutomate(Exception ex)
    {
        if (this.Disabled)
            return;
        this.Disabled = true;

        this.Monitor.Log($"Hopper Brain could not patch Automate (incompatible Automate version?). Automate has been disabled so plain chests do not automate. Details: {ex}", LogLevel.Error);

        try
        {
            IModInfo info = this.Helper.ModRegistry.Get("Pathoschild.Automate") ?? throw new InvalidOperationException("Automate isn't loaded.");
            object mod = info.GetType().GetProperty("Mod")?.GetValue(info) ?? throw new InvalidOperationException("Can't access Automate's mod instance.");
            object config = this.Helper.Reflection.GetField<object>(mod, "Config").GetValue();
            this.Helper.Reflection.GetProperty<bool>(config, "Enabled").SetValue(false);
            this.Helper.Reflection.GetMethod(mod, "ReloadConfig").Invoke();
        }
        catch (Exception disableEx)
        {
            this.Monitor.Log($"Hopper Brain also failed to disable Automate. Plain chests may automate. Remove Automate or Hopper Brain until they're compatible. Details: {disableEx}", LogLevel.Error);
        }
    }
}
