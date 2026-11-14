using HopperBrain;

namespace HopperBrain.Tests;

public class ModConfigTests
{
    [Fact]
    public void Normalize_UndefinedEnums_ResetToVanilla()
    {
        var config = new ModConfig { RecipeCost = (RecipeCost)7, RecipeUnlock = (RecipeUnlock)42 };
        config.Normalize();
        Assert.Equal(RecipeCost.Vanilla, config.RecipeCost);
        Assert.Equal(RecipeUnlock.Vanilla, config.RecipeUnlock);
    }

    [Fact]
    public void Normalize_NegativePrices_ClampToZero()
    {
        var config = new ModConfig { RobinRecipePrice = -1, RobinHopperPrice = -50 };
        config.Normalize();
        Assert.Equal(0, config.RobinRecipePrice);
        Assert.Equal(0, config.RobinHopperPrice);
    }

    [Fact]
    public void Normalize_ValidValues_Unchanged()
    {
        var config = new ModConfig { RecipeCost = RecipeCost.Iron, RecipeUnlock = RecipeUnlock.Robin, RobinRecipePrice = 123, RobinHopperPrice = 45 };
        config.Normalize();
        Assert.Equal(RecipeCost.Iron, config.RecipeCost);
        Assert.Equal(RecipeUnlock.Robin, config.RecipeUnlock);
        Assert.Equal(123, config.RobinRecipePrice);
        Assert.Equal(45, config.RobinHopperPrice);
    }
}
