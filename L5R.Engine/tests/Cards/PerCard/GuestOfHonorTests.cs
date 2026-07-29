using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class GuestOfHonorTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "guest-of-honor.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void WhileParticipating_TheOpponentCannotPlayEvents()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var guest = new Card { Id = "guest-of-honor", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        p1.PlayArea.Add(guest);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(guest);
        game.CurrentConflict = conflict;

        var someEvent = new Card { Id = "some-event", Type = CardType.Event, Controller = p2 };
        var someCharacter = new Card { Id = "some-character", Type = CardType.Character, Controller = p2 };

        Assert.That(game.IsPlayerRestrictedFrom(p2, "play", someEvent), Is.True);
        Assert.That(game.IsPlayerRestrictedFrom(p2, "play", someCharacter), Is.False, "restricts: events only covers event cards");
        Assert.That(game.IsPlayerRestrictedFrom(p1, "play", someEvent), Is.False, "the restriction targets the opponent, not guest-of-honor's own controller");
    }

    [Test]
    public void WhileNotParticipating_NoRestriction()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var guest = new Card { Id = "guest-of-honor", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        p1.PlayArea.Add(guest);

        var someEvent = new Card { Id = "some-event", Type = CardType.Event, Controller = p2 };

        Assert.That(game.IsPlayerRestrictedFrom(p2, "play", someEvent), Is.False);
    }
}
