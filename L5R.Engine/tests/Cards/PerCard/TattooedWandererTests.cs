using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class TattooedWandererTests
{
    private static IReadOnlyList<WhileAttachedDefinition> LoadWhileAttached()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "tattooed-wanderer.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseWhileAttached(document.RootElement);
    }

    [Test]
    public void WhilePlayedAsAnAttachment_GrantsTheCovertKeyword()
    {
        // Its alternate "play as an attachment" mode is exercised directly below via
        // TattooedWandererPlayAsAttachment; this test just confirms the whileAttached effect
        // itself (plain JSON) applies once attached, regardless of how it got there.
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var host = new Card { Id = "some-character", Type = CardType.Character, Controller = p1 };
        var wanderer = new Card
        {
            Id = "tattooed-wanderer", Type = CardType.Character, Controller = p1,
            AttachedTo = host, WhileAttachedEffects = LoadWhileAttached()
        };
        p1.PlayArea.Add(host);
        p1.PlayArea.Add(wanderer);

        Assert.That(game.HasKeyword(host, "covert"), Is.True);
    }

    [Test]
    public void PlaysFromHandAsAnAttachment_LeavingItsPrintedTypeAsCharacter()
    {
        var p1 = new Player { Name = "Player1", Fate = 3 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var wanderer = new Card { Id = "tattooed-wanderer", Type = CardType.Character, Controller = p1, PrintedCost = 1 };
        var host = new Card { Id = "host", Type = CardType.Character, Controller = p1 };
        p1.Hand.Add(wanderer);
        p1.PlayArea.Add(host);

        var context = new AbilityContext { Game = game, Player = p1, Source = wanderer, PlayAttachTarget = host };

        new TattooedWandererPlayAsAttachment().Execute(context);

        Assert.That(wanderer.AttachedTo, Is.EqualTo(host));
        Assert.That(wanderer.Type, Is.EqualTo(CardType.Character), "printed type is never mutated - AttachedTo is the only fact that changes");
        Assert.That(p1.PlayArea, Contains.Item(wanderer));
        Assert.That(p1.Fate, Is.EqualTo(2));
    }

    [Test]
    public void WithoutAnAttachTarget_Throws()
    {
        var p1 = new Player { Name = "Player1", Fate = 3 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var wanderer = new Card { Id = "tattooed-wanderer", Type = CardType.Character, Controller = p1, PrintedCost = 1 };
        p1.Hand.Add(wanderer);

        var context = new AbilityContext { Game = game, Player = p1, Source = wanderer };

        Assert.Throws<InvalidOperationException>(() => new TattooedWandererPlayAsAttachment().Execute(context));
    }
}
