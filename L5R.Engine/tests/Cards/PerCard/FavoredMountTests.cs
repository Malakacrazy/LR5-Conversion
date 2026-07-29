using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class FavoredMountTests
{
    private static IReadOnlyList<WhileAttachedDefinition> LoadWhileAttached()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "favored-mount.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseWhileAttached(document.RootElement);
    }

    [Test]
    public void WhileAttached_GrantsTheCavalryTrait()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rider = new Card { Id = "some-bushi", Type = CardType.Character, Controller = p1 };
        var mount = new Card
        {
            Id = "favored-mount", Type = CardType.Attachment, Controller = p1,
            AttachedTo = rider, WhileAttachedEffects = LoadWhileAttached()
        };
        p1.PlayArea.Add(rider);
        p1.PlayArea.Add(mount);

        Assert.That(rider.Traits, Does.Not.Contain("cavalry"), "cavalry isn't a printed trait");
        Assert.That(game.HasEffectiveTrait(rider, "cavalry"), Is.True);
    }

    [Test]
    public void WhenNotAttached_DoesNotGrantTheTrait()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rider = new Card { Id = "some-bushi", Type = CardType.Character, Controller = p1 };
        var mount = new Card
        {
            Id = "favored-mount", Type = CardType.Attachment, Controller = p1,
            WhileAttachedEffects = LoadWhileAttached()
        };
        p1.PlayArea.Add(rider);
        p1.PlayArea.Add(mount);

        Assert.That(game.HasEffectiveTrait(rider, "cavalry"), Is.False);
    }
}
