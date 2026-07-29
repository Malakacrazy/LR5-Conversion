using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class DeathseekerTests
{
    private static (GameState Game, Card Deathseeker) NewGameLostWhileAttacking()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var deathseeker = new Card { Id = "deathseeker", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(deathseeker);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Loser = p1 };
        conflict.Attackers.Add(deathseeker);
        game.CurrentConflict = conflict;

        return (game, deathseeker);
    }

    [Test]
    public void SacrificesItselfToRemoveFateFromATargetThatHasSome()
    {
        var (game, deathseeker) = NewGameLostWhileAttacking();
        var target = new Card { Id = "opponent-character", Type = CardType.Character, Controller = game.Player2, Fate = 2 };
        game.Player2.PlayArea.Add(target);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = deathseeker, Target = target };

        new DeathseekerRemoveFateOrDiscardOnLoss().Execute(context);

        Assert.That(game.Player1.Discard, Does.Contain(deathseeker), "sacrificed as the cost");
        Assert.That(target.Fate, Is.EqualTo(1));
        Assert.That(game.Player2.PlayArea, Does.Contain(target), "not discarded, just lost fate");
    }

    [Test]
    public void SacrificesItselfToDiscardATargetWithNoFate()
    {
        var (game, deathseeker) = NewGameLostWhileAttacking();
        var target = new Card { Id = "opponent-character", Type = CardType.Character, Controller = game.Player2, Fate = 0 };
        game.Player2.PlayArea.Add(target);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = deathseeker, Target = target };

        new DeathseekerRemoveFateOrDiscardOnLoss().Execute(context);

        Assert.That(game.Player2.Discard, Does.Contain(target));
    }

    [Test]
    public void WhenTheControllerDoesNotLose_Throws()
    {
        var (game, deathseeker) = NewGameLostWhileAttacking();
        game.CurrentConflict!.Loser = game.Player2;
        var target = new Card { Id = "opponent-character", Type = CardType.Character, Controller = game.Player2, Fate = 1 };
        game.Player2.PlayArea.Add(target);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = deathseeker, Target = target };

        Assert.Throws<InvalidOperationException>(() => new DeathseekerRemoveFateOrDiscardOnLoss().Execute(context));
        Assert.That(game.Player1.PlayArea, Does.Contain(deathseeker), "not sacrificed");
    }

    [Test]
    public void WhileDefending_Throws()
    {
        var (game, deathseeker) = NewGameLostWhileAttacking();
        game.CurrentConflict!.Attackers.Remove(deathseeker);
        game.CurrentConflict!.Defenders.Add(deathseeker);
        var target = new Card { Id = "opponent-character", Type = CardType.Character, Controller = game.Player2, Fate = 1 };
        game.Player2.PlayArea.Add(target);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = deathseeker, Target = target };

        Assert.Throws<InvalidOperationException>(() => new DeathseekerRemoveFateOrDiscardOnLoss().Execute(context));
    }
}
