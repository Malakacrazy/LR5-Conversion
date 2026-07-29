using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class MagnificentKimonoTests
{
    private static IReadOnlyList<WhileAttachedDefinition> LoadWhileAttached()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "magnificent-kimono.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseWhileAttached(document.RootElement);
    }

    [Test]
    public void WhileAttached_GrantsThePrideKeyword()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var wearer = new Card { Id = "some-courtier", Type = CardType.Character, Controller = p1 };
        var kimono = new Card
        {
            Id = "magnificent-kimono", Type = CardType.Attachment, Controller = p1,
            AttachedTo = wearer, WhileAttachedEffects = LoadWhileAttached()
        };
        p1.PlayArea.Add(wearer);
        p1.PlayArea.Add(kimono);

        Assert.That(game.HasKeyword(wearer, "pride"), Is.True);
    }

    [Test]
    public void WhenNotAttached_DoesNotGrantTheKeyword()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var wearer = new Card { Id = "some-courtier", Type = CardType.Character, Controller = p1 };
        var kimono = new Card
        {
            Id = "magnificent-kimono", Type = CardType.Attachment, Controller = p1,
            WhileAttachedEffects = LoadWhileAttached()
        };
        p1.PlayArea.Add(wearer);
        p1.PlayArea.Add(kimono);

        Assert.That(game.HasKeyword(wearer, "pride"), Is.False);
    }
}
