using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.State;

/// <summary>
/// akodo-toturi/doji-hotaru/defend-the-wall all share this one gameAction
/// ("resolveConflictRing"), so its 5 element branches are proven directly here rather than
/// duplicated per card - the same pattern already used for RingGameActionTests.
/// </summary>
public class ResolveConflictRingGameActionTests
{
    private static (GameState Game, Card Source) NewGameWithConflict(string element)
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2", Honor = 3 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "ring-resolver", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(source);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Elements = new List<string> { element } };
        game.CurrentConflict = conflict;

        return (game, source);
    }

    [Test]
    public void Air_GainTwoHonor_GainsHonor()
    {
        var (game, source) = NewGameWithConflict("air");
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, ChosenChoice = "Gain 2 Honor" };

        new ResolveConflictRingGameActionHandler().Execute(context, null);

        Assert.That(game.Player1.Honor, Is.EqualTo(5));
    }

    [Test]
    public void Air_TakeHonorFromOpponent_TransfersHonor()
    {
        var (game, source) = NewGameWithConflict("air");
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, ChosenChoice = "Take 1 Honor from opponent" };

        new ResolveConflictRingGameActionHandler().Execute(context, null);

        Assert.That(game.Player1.Honor, Is.EqualTo(4));
        Assert.That(game.Player2.Honor, Is.EqualTo(2));
    }

    [Test]
    public void Air_NullChoice_DoesNotResolve()
    {
        var (game, source) = NewGameWithConflict("air");
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source };

        new ResolveConflictRingGameActionHandler().Execute(context, null);

        Assert.That(game.Player1.Honor, Is.EqualTo(3));
    }

    [Test]
    public void Earth_DrawsACardAndOpponentDiscardsOne()
    {
        var (game, source) = NewGameWithConflict("earth");
        game.Player1.Deck.Add(new Card { Id = "top-card", Type = CardType.Character, Controller = game.Player1 });
        var opponentCard = new Card { Id = "opponent-hand-card", Type = CardType.Character, Controller = game.Player2, Location = "hand" };
        game.Player2.Hand.Add(opponentCard);

        var context = new AbilityContext
        {
            Game = game, Player = game.Player1, Source = source, ChosenChoice = "Draw a card and opponent discards",
            ChosenDiscardCards = new[] { opponentCard }
        };

        new ResolveConflictRingGameActionHandler().Execute(context, null);

        Assert.That(game.Player1.Hand, Has.Count.EqualTo(1));
        Assert.That(game.Player2.Discard, Does.Contain(opponentCard));
    }

    [Test]
    public void Fire_Honor_HonorsTheTarget()
    {
        var (game, source) = NewGameWithConflict("fire");
        var target = new Card { Id = "target", Type = CardType.Character, Controller = game.Player2 };

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, Target = target, ChosenChoice = "Honor" };

        new ResolveConflictRingGameActionHandler().Execute(context, null);

        Assert.That(target.IsHonored, Is.True);
    }

    [Test]
    public void Fire_Dishonor_DishonorsTheTarget()
    {
        var (game, source) = NewGameWithConflict("fire");
        var target = new Card { Id = "target", Type = CardType.Character, Controller = game.Player2 };

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, Target = target, ChosenChoice = "Dishonor" };

        new ResolveConflictRingGameActionHandler().Execute(context, null);

        Assert.That(target.IsDishonored, Is.True);
    }

    [Test]
    public void Water_BowsAFatelessUnbowedTarget()
    {
        var (game, source) = NewGameWithConflict("water");
        var target = new Card { Id = "target", Type = CardType.Character, Controller = game.Player2, Fate = 0 };

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, Target = target };

        new ResolveConflictRingGameActionHandler().Execute(context, null);

        Assert.That(target.Bowed, Is.True);
    }

    [Test]
    public void Water_ReadiesABowedTargetRegardlessOfFate()
    {
        var (game, source) = NewGameWithConflict("water");
        var target = new Card { Id = "target", Type = CardType.Character, Controller = game.Player2, Fate = 2, Bowed = true };

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, Target = target };

        new ResolveConflictRingGameActionHandler().Execute(context, null);

        Assert.That(target.Bowed, Is.False);
    }

    [Test]
    public void Water_BowingATargetWithFate_Throws()
    {
        var (game, source) = NewGameWithConflict("water");
        var target = new Card { Id = "target", Type = CardType.Character, Controller = game.Player2, Fate = 2 };

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, Target = target };

        Assert.Throws<InvalidOperationException>(() => new ResolveConflictRingGameActionHandler().Execute(context, null));
    }

    [Test]
    public void Void_RemovesFateFromTheTarget()
    {
        var (game, source) = NewGameWithConflict("void");
        var target = new Card { Id = "target", Type = CardType.Character, Controller = game.Player2, Fate = 2 };

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, Target = target };

        new ResolveConflictRingGameActionHandler().Execute(context, null);

        Assert.That(target.Fate, Is.EqualTo(1));
    }

    [Test]
    public void Void_NullTarget_DoesNotResolve()
    {
        var (game, source) = NewGameWithConflict("void");
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source };

        new ResolveConflictRingGameActionHandler().Execute(context, null);
    }

    [Test]
    public void WithNoActiveConflict_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "ring-resolver", Type = CardType.Character, Controller = p1 };

        var context = new AbilityContext { Game = game, Player = p1, Source = source };

        Assert.Throws<InvalidOperationException>(() => new ResolveConflictRingGameActionHandler().Execute(context, null));
    }
}
