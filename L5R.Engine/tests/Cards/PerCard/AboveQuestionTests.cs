using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class AboveQuestionTests
{
    private static IReadOnlyList<WhileAttachedDefinition> LoadWhileAttached()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "above-question.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseWhileAttached(document.RootElement);
    }

    private static (GameState Game, Card Protected) NewAttachedGame()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var courtier = new Card { Id = "protected-courtier", Type = CardType.Character, Controller = p1 };
        var mask = new Card { Id = "above-question", Type = CardType.Attachment, Controller = p1, AttachedTo = courtier, WhileAttachedEffects = LoadWhileAttached() };
        p1.PlayArea.Add(courtier);
        p1.PlayArea.Add(mask);
        return (game, courtier);
    }

    [Test]
    public void CannotBeTargetedByAnOpponentsEvent()
    {
        var (game, courtier) = NewAttachedGame();
        var opponentEvent = new Card { Id = "opponent-event", Type = CardType.Event, Controller = game.Player2 };

        Assert.That(game.IsRestrictedFrom(courtier, "target", opponentEvent), Is.True);
    }

    [Test]
    public void CanStillBeTargetedByItsControllersOwnEvent()
    {
        var (game, courtier) = NewAttachedGame();
        var ownEvent = new Card { Id = "own-event", Type = CardType.Event, Controller = game.Player1 };

        Assert.That(game.IsRestrictedFrom(courtier, "target", ownEvent), Is.False);
    }

    [Test]
    public void CanStillBeTargetedByAnOpponentsNonEventAbility()
    {
        var (game, courtier) = NewAttachedGame();
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = game.Player2 };

        Assert.That(game.IsRestrictedFrom(courtier, "target", opponentCharacter), Is.False, "only restricts events, not character abilities");
    }

    [Test]
    public void WhenNotAttached_NotRestricted()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var courtier = new Card { Id = "unprotected-courtier", Type = CardType.Character, Controller = p1 };
        var mask = new Card { Id = "above-question", Type = CardType.Attachment, Controller = p1, WhileAttachedEffects = LoadWhileAttached() };
        p1.PlayArea.Add(courtier);
        p1.PlayArea.Add(mask);
        var opponentEvent = new Card { Id = "opponent-event", Type = CardType.Event, Controller = p2 };

        Assert.That(game.IsRestrictedFrom(courtier, "target", opponentEvent), Is.False);
    }
}
