namespace HopperBrain;

/// <summary>The mod settings.</summary>
internal sealed class ModConfig
{
    /// <summary>The hopper recipe cost preset.</summary>
    public RecipeCost RecipeCost { get; set; } = RecipeCost.Vanilla;

    /// <summary>How the hopper recipe is unlocked.</summary>
    public RecipeUnlock RecipeUnlock { get; set; } = RecipeUnlock.Vanilla;

    /// <summary>Robin's price for the hopper recipe, when <see cref="RecipeUnlock"/> is <see cref="RecipeUnlock.Robin"/>.</summary>
    public int RobinRecipePrice { get; set; } = 50000;

    /// <summary>Robin's price for a hopper, when <see cref="RecipeUnlock"/> is <see cref="RecipeUnlock.Robin"/>.</summary>
    public int RobinHopperPrice { get; set; } = 10000;
}
