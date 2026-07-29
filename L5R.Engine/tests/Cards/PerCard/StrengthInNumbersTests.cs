using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class StrengthInNumbersTests
{
    private static (GameState Game, Card Source, Card Defender) NewGameAttackingWithTwoCharacters(int defenderGlory = 2)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "strength-in-numbers", Type = CardType.Event, Controller = p1 };
        var attacker1 = new Card { Id = "attacker-1", Type = CardType.Character, Controller = p1 };
        var attacker2 = new Card { Id = "attacker-2", Type = CardType.Character, Controller = p1 };
        var defender = new Card { Id = "defender", Type = CardType.Character, Controller = p2, PrintedGlory = defenderGlory };
        p1.PlayArea.Add(attacker1);
        p1.PlayArea.Add(attacker2);
        p2.PlayArea.Add(defender);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(attacker1);
        conflict.Attackers.Add(attacker2);
        conflict.Defenders.Add(defender);
        game.CurrentConflict = conflict;

        return (game, source, defender);
    }

    [Test]
    public void SendsHomeADefenderWithGloryAtOrBelowTheAttackerCount()
    {
        var (game, source, defender) = NewGameAttackingWithTwoCharacters();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, Target = defender };

        new StrengthInNumbersSendHomeLowGloryDefender().Execute(context);

        Assert.That(game.CurrentConflict!.Defenders, Does.Not.Contain(defender));
    }

    [Test]
    public void ADefenderWithTooMuchGlory_Throws()
    {
        var (game, source, defender) = NewGameAttackingWithTwoCharacters(defenderGlory: 3);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, Target = defender };

        Assert.Throws<InvalidOperationException>(() => new StrengthInNumbersSendHomeLowGloryDefender().Execute(context));
    }

    [Test]
    public void WhenTheControllerIsNotAttacking_Throws()
    {
        var (game, source, defender) = NewGameAttackingWithTwoCharacters();
        var context = new AbilityContext { Game = game, Player = game.Player2, Source = source, Target = defender };

        Assert.Throws<InvalidOperationException>(() => new StrengthInNumbersSendHomeLowGloryDefender().Execute(context));
    }
}
