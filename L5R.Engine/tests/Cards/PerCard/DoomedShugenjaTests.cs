using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class DoomedShugenjaTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "doomed-shugenja.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void ItsControllerCannotPlaceFateWhenPlayingItSpecifically()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var shugenja = new Card { Id = "doomed-shugenja", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        // sourceLocation "any" - active even while doomed-shugenja itself is in hand, not in play.
        p1.Hand.Add(shugenja);

        Assert.That(game.IsPlayerRestrictedFrom(p1, "placeFateWhenPlayingCharacter", shugenja), Is.True);
    }

    [Test]
    public void DoesNotRestrictPlacingFateOnADifferentCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var shugenja = new Card { Id = "doomed-shugenja", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        p1.Hand.Add(shugenja);

        var otherCharacter = new Card { Id = "some-other-character", Type = CardType.Character, Controller = p1 };

        Assert.That(game.IsPlayerRestrictedFrom(p1, "placeFateWhenPlayingCharacter", otherCharacter), Is.False, "restricts: source only covers doomed-shugenja itself");
    }

    [Test]
    public void DoesNotRestrictTheOpponent()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var shugenja = new Card { Id = "doomed-shugenja", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        p1.Hand.Add(shugenja);

        Assert.That(game.IsPlayerRestrictedFrom(p2, "placeFateWhenPlayingCharacter", shugenja), Is.False, "targetController defaults to self - only doomed-shugenja's own controller is restricted");
    }
}
