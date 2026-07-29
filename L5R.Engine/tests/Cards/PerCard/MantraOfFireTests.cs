using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class MantraOfFireTests
{
    private static (GameState Game, Card Mantra) NewGameWithFireConflictFromOpponent()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var mantra = new Card { Id = "mantra-of-fire", Type = CardType.Event, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Elements = new List<string> { "fire" } };
        game.CurrentConflict = conflict;

        return (game, mantra);
    }

    [Test]
    public void OnAMonk_PlacesFateAndDraws()
    {
        var (game, mantra) = NewGameWithFireConflictFromOpponent();
        var monk = new Card { Id = "some-monk", Type = CardType.Character, Controller = game.Player1, Traits = new[] { "monk" } };
        game.Player1.PlayArea.Add(monk);
        game.Player1.Deck.Add(new Card { Id = "top-card", Type = CardType.Character, Controller = game.Player1 });

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = mantra, Target = monk };

        new MantraOfFireAddFateToMonkAndDraw().Execute(context);

        Assert.That(monk.Fate, Is.EqualTo(1));
        Assert.That(game.Player1.Hand, Has.Count.EqualTo(1));
    }

    [Test]
    public void OnACharacterWithAMonkAttachment_PlacesFate()
    {
        var (game, mantra) = NewGameWithFireConflictFromOpponent();
        var character = new Card { Id = "non-monk", Type = CardType.Character, Controller = game.Player1 };
        var monkAttachment = new Card { Id = "monk-robes", Type = CardType.Attachment, Controller = game.Player1, Traits = new[] { "monk" }, AttachedTo = character };
        game.Player1.PlayArea.Add(character);
        game.Player1.PlayArea.Add(monkAttachment);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = mantra, Target = character };

        new MantraOfFireAddFateToMonkAndDraw().Execute(context);

        Assert.That(character.Fate, Is.EqualTo(1));
    }

    [Test]
    public void OnANonMonkWithNoMonkAttachment_Throws()
    {
        var (game, mantra) = NewGameWithFireConflictFromOpponent();
        var character = new Card { Id = "non-monk", Type = CardType.Character, Controller = game.Player1 };
        game.Player1.PlayArea.Add(character);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = mantra, Target = character };

        Assert.Throws<InvalidOperationException>(() => new MantraOfFireAddFateToMonkAndDraw().Execute(context));
    }

    [Test]
    public void WhenTheConflictIsDeclaredByItsOwnController_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var mantra = new Card { Id = "mantra-of-fire", Type = CardType.Event, Controller = p1 };
        var monk = new Card { Id = "some-monk", Type = CardType.Character, Controller = p1, Traits = new[] { "monk" } };
        p1.PlayArea.Add(monk);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Elements = new List<string> { "fire" } };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = mantra, Target = monk };

        Assert.Throws<InvalidOperationException>(() => new MantraOfFireAddFateToMonkAndDraw().Execute(context));
    }
}
