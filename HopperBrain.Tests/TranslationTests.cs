using System.Text.Json;
using HopperBrain;

namespace HopperBrain.Tests;

public class TranslationTests
{
    private static Dictionary<string, string> Load(string file) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "i18n", file)),
            new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true })!;

    [Fact]
    public void Default_HasKeyForEveryOption()
    {
        var keys = Load("default.json").Keys;
        foreach (string name in Enum.GetNames<RecipeCost>())
            Assert.Contains($"config.cost.{name}", keys);
        foreach (string name in Enum.GetNames<RecipeUnlock>())
            Assert.Contains($"config.unlock.{name}", keys);
    }

    [Fact]
    public void PtBr_HasSameKeysAsDefault() =>
        Assert.Equal(Load("default.json").Keys.Order(), Load("pt-BR.json").Keys.Order());
}
