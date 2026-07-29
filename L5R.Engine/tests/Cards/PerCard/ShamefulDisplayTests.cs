using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ShamefulDisplayTests
{
    private static (GameState Game, Card ShamefulDisplay, Card First, Card Second) NewGameBothParticipating()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var shamefulDisplay = new Card { Id = "shameful-display", Type = CardType.Province, Controller = p1 };
        var first = new Card { Id = "first-character", Type = CardType.Character, Controller = p1 };
        var second = new Card { Id = "second-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(first);
        p2.PlayArea.Add(second);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(first);
        conflict.Defenders.Add(second);
        game.CurrentConflict = conflict;

        return (game, shamefulDisplay, first, second);
    }

    [Test]
    public void HonorsOneAndDishonorsTheOther()
    {
        var (game, shamefulDisplay, first, second) = NewGameBothParticipating();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = shamefulDisplay, Target = first, SecondTarget = second };

        new ShamefulDisplayHonorOneDishonorOther().Execute(context);

        Assert.That(first.IsHonored, Is.True);
        Assert.That(second.IsDishonored, Is.True);
    }

    [Test]
    public void WithTheSameCharacterTwice_Throws()
    {
        var (game, shamefulDisplay, first, _) = NewGameBothParticipating();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = shamefulDisplay, Target = first, SecondTarget = first };

        Assert.Throws<InvalidOperationException>(() => new ShamefulDisplayHonorOneDishonorOther().Execute(context));
    }

    [Test]
    public void WithANonParticipatingCharacter_Throws()
    {
        var (game, shamefulDisplay, first, _) = NewGameBothParticipating();
        var nonParticipant = new Card { Id = "non-participant", Type = CardType.Character, Controller = game.Player1 };
        game.Player1.PlayArea.Add(nonParticipant);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = shamefulDisplay, Target = first, SecondTarget = nonParticipant };

        Assert.Throws<InvalidOperationException>(() => new ShamefulDisplayHonorOneDishonorOther().Execute(context));
    }
}
