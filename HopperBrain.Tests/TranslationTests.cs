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

    [Theory]
    [InlineData("pt-BR.json")]
    [InlineData("zh.json")]
    [InlineData("ru.json")]
    [InlineData("es.json")]
    public void Translation_HasSameKeysAsDefault(string file) =>
        Assert.Equal(Load("default.json").Keys.Order(), Load(file).Keys.Order());
}
