using HopperBrain;

namespace HopperBrain.Tests;

public class HopperRecipeTests
{
    [Fact]
    public void Ingredients_Vanilla_IsNull() => Assert.Null(HopperRecipe.Ingredients(RecipeCost.Vanilla));

    [Theory]
    [InlineData(RecipeCost.Copper, "388 50 334 5")]
    [InlineData(RecipeCost.Iron, "709 10 335 5")]
    [InlineData(RecipeCost.Gold, "709 10 336 5 330 3")]
    [InlineData(RecipeCost.Iridium, "709 10 337 1 787 1")]
    public void Ingredients_MatchSpec(RecipeCost cost, string expected) => Assert.Equal(expected, HopperRecipe.Ingredients(cost));

    [Theory]
    [InlineData(RecipeUnlock.Start, 0, true)]
    [InlineData(RecipeUnlock.Farming4, 3, false)]
    [InlineData(RecipeUnlock.Farming4, 4, true)]
    [InlineData(RecipeUnlock.Farming9, 8, false)]
    [InlineData(RecipeUnlock.Farming9, 9, true)]
    [InlineData(RecipeUnlock.Vanilla, 10, false)]
    [InlineData(RecipeUnlock.Robin, 10, false)]
    public void ShouldKnow(RecipeUnlock unlock, int level, bool expected) => Assert.Equal(expected, HopperRecipe.ShouldKnow(unlock, level));

    [Fact]
    public void ClampPrice_Negative_IsZero()
    {
        Assert.Equal(0, HopperRecipe.ClampPrice(-5));
        Assert.Equal(50000, HopperRecipe.ClampPrice(50000));
    }
}
