using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class AsakoDiplomatTests
{
    private static (GameState Game, Card Diplomat, Card Target) NewGameWonWhileParticipating()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var diplomat = new Card { Id = "asako-diplomat", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "some-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(diplomat);
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Winner = p1 };
        conflict.Attackers.Add(diplomat);
        game.CurrentConflict = conflict;

        return (game, diplomat, target);
    }

    [Test]
    public void ChoosingHonor_HonorsTheChosenCharacter()
    {
        var (game, diplomat, target) = NewGameWonWhileParticipating();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = diplomat, Target = target, ChosenChoice = "Honor this character" };

        new AsakoDiplomatHonorOrDishonorOnWin().Execute(context);

        Assert.That(target.IsHonored, Is.True);
    }

    [Test]
    public void ChoosingDishonor_DishonorsTheChosenCharacterInstead()
    {
        var (game, diplomat, target) = NewGameWonWhileParticipating();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = diplomat, Target = target, ChosenChoice = "Dishonor this character" };

        new AsakoDiplomatHonorOrDishonorOnWin().Execute(context);

        Assert.That(target.IsDishonored, Is.True);
    }

    [Test]
    public void WhenTheControllerDoesNotWin_Throws()
    {
        var (game, diplomat, target) = NewGameWonWhileParticipating();
        game.CurrentConflict!.Winner = game.Player2;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = diplomat, Target = target, ChosenChoice = "Honor this character" };

        Assert.Throws<InvalidOperationException>(() => new AsakoDiplomatHonorOrDishonorOnWin().Execute(context));
    }

    [Test]
    public void WithoutAChosenChoice_Throws()
    {
        var (game, diplomat, target) = NewGameWonWhileParticipating();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = diplomat, Target = target };

        Assert.Throws<InvalidOperationException>(() => new AsakoDiplomatHonorOrDishonorOnWin().Execute(context));
    }
}
