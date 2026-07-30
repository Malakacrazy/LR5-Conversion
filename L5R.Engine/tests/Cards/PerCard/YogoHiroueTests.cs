using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class YogoHiroueTests
{
    private static (GameState Game, Card Hiroue, Card Target, Conflict Conflict) NewGameHirouParticipating()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var hiroue = new Card { Id = "yogo-hiroue", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "some-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(hiroue);
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(hiroue);
        game.CurrentConflict = conflict;

        return (game, hiroue, target, conflict);
    }

    [Test]
    public void Execute_MovesTheTargetIntoTheConflict()
    {
        var (game, hiroue, target, conflict) = NewGameHirouParticipating();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = hiroue, Target = target };

        new YogoHiroueMoveThenMayDishonorOnWin().Execute(context);

        Assert.That(conflict.Defenders, Does.Contain(target));
    }

    [Test]
    public void Execute_WhenNotParticipating_Throws()
    {
        var (game, hiroue, target, conflict) = NewGameHirouParticipating();
        conflict.Attackers.Remove(hiroue);
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = hiroue, Target = target };

        Assert.Throws<InvalidOperationException>(() => new YogoHiroueMoveThenMayDishonorOnWin().Execute(context));
    }

    [Test]
    public void ResolveDishonorChoice_WhenWonAndChoosingYes_DishonorsTheTarget()
    {
        var (game, hiroue, target, conflict) = NewGameHirouParticipating();
        conflict.Winner = game.Player1;
        game.EndConflict();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = hiroue, Target = target };

        new YogoHiroueMoveThenMayDishonorOnWin().ResolveDishonorChoice(context, conflict, choseToDishonor: true);

        Assert.That(target.IsDishonored, Is.True);
    }

    [Test]
    public void ResolveDishonorChoice_WhenWonAndChoosingNo_DoesNotDishonor()
    {
        var (game, hiroue, target, conflict) = NewGameHirouParticipating();
        conflict.Winner = game.Player1;
        game.EndConflict();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = hiroue, Target = target };

        new YogoHiroueMoveThenMayDishonorOnWin().ResolveDishonorChoice(context, conflict, choseToDishonor: false);

        Assert.That(target.IsDishonored, Is.False);
    }

    [Test]
    public void ResolveDishonorChoice_WhenNotWon_Throws()
    {
        var (game, hiroue, target, conflict) = NewGameHirouParticipating();
        conflict.Winner = game.Player2;
        game.EndConflict();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = hiroue, Target = target };

        Assert.Throws<InvalidOperationException>(() => new YogoHiroueMoveThenMayDishonorOnWin().ResolveDishonorChoice(context, conflict, choseToDishonor: true));
    }
}
