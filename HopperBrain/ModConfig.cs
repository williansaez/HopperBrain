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

    /// <summary>Reset unknown options to vanilla and clamp prices, so a hand-edited config can't break the game.</summary>
    public void Normalize()
    {
        if (!System.Enum.IsDefined(this.RecipeCost))
            this.RecipeCost = RecipeCost.Vanilla;
        if (!System.Enum.IsDefined(this.RecipeUnlock))
            this.RecipeUnlock = RecipeUnlock.Vanilla;
        this.RobinRecipePrice = HopperRecipe.ClampPrice(this.RobinRecipePrice);
        this.RobinHopperPrice = HopperRecipe.ClampPrice(this.RobinHopperPrice);
    }
}
