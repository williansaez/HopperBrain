using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.BigCraftables;

namespace HopperBrain;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod
{
    /// <summary>Automate's context tag which makes a storage take-only.</summary>
    private const string TakeOnlyTag = "automate_storage_take";

    /// <summary>Whether Automate was already disabled, to avoid repeating the error.</summary>
    private bool Disabled;

    public override void Entry(IModHelper helper)
    {
        helper.Events.Content.AssetRequested += this.OnAssetRequested;

        try
        {
            AutomatePatches.Apply(new Harmony(this.ModManifest.UniqueID), this.DisableAutomate);
        }
        catch (Exception ex)
        {
            this.DisableAutomate(ex);
        }
    }

    /// <summary>Let the hopper store items too (Automate marks it take-only).</summary>
    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (!e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
            return;

        // late priority so this runs after Automate adds the tag
        e.Edit(asset =>
        {
            var data = asset.AsDictionary<string, BigCraftableData>().Data;
            if (data.TryGetValue("275", out BigCraftableData? hopper))
                hopper.ContextTags?.RemoveAll(tag => tag == TakeOnlyTag);
        }, AssetEditPriority.Late);
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
