using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class MeishodoWielderTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "meishodo-wielder.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void WhileItsControllerIsTheFirstPlayer_CostsOneLess()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var wielder = new Card
        {
            Id = "meishodo-wielder", Type = CardType.Character, Controller = p1,
            PrintedCost = 2, Location = "hand", PersistentEffects = LoadPersistentEffects()
        };
        p1.Hand.Add(wielder);

        Assert.That(game.EffectiveCost(wielder, p1), Is.EqualTo(1));
    }

    [Test]
    public void WhileItsControllerIsNotTheFirstPlayer_CostsFullPrice()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var wielder = new Card
        {
            Id = "meishodo-wielder", Type = CardType.Character, Controller = p1,
            PrintedCost = 2, Location = "hand", PersistentEffects = LoadPersistentEffects()
        };
        p1.Hand.Add(wielder);

        Assert.That(game.EffectiveCost(wielder, p1), Is.EqualTo(2));
    }
}
