using System;

namespace HopperBrain;

/// <summary>The hopper recipe cost preset.</summary>
public enum RecipeCost { Vanilla, Copper, Iron, Gold, Iridium }

/// <summary>How the hopper recipe is unlocked.</summary>
public enum RecipeUnlock { Vanilla, Start, Farming4, Farming9, Robin }

/// <summary>Maps config options to recipe data. Pure logic, no game types.</summary>
public static class HopperRecipe
{
    /// <summary>The recipe ingredients as "id count" pairs, or null to keep the game's recipe.</summary>
    public static string? Ingredients(RecipeCost cost) => cost switch
    {
        RecipeCost.Copper => "388 50 334 5",           // wood, copper bar
        RecipeCost.Iron => "709 10 335 5",             // hardwood, iron bar
        RecipeCost.Gold => "709 10 336 5 330 3",       // hardwood, gold bar, clay
        RecipeCost.Iridium => "709 10 337 1 787 1",    // hardwood, iridium bar, battery pack
        _ => null
    };

    /// <summary>Whether the player should already know the recipe under this unlock rule.</summary>
    public static bool ShouldKnow(RecipeUnlock unlock, int farmingLevel) => unlock switch
    {
        RecipeUnlock.Start => true,
        RecipeUnlock.Farming4 => farmingLevel >= 4,
        RecipeUnlock.Farming9 => farmingLevel >= 9,
        _ => false
    };

    /// <summary>Keep shop prices non-negative.</summary>
    public static int ClampPrice(int price) => Math.Max(0, price);
}
