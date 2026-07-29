using System.Text.Json;
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
        // Its alternate "play as an attachment" mode is scriptOverride'd (out of scope - a
        // permanent type reclassification the generic schema has no concept of), but the
        // whileAttached effect itself is plain JSON, testable once attached by any means.
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
}
