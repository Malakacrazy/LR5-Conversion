using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class HidaTomonatsuTests
{
    private static (GameState Game, Card Tomonatsu, Card Attacker) NewGameWonWhileDefending(bool attackerUnique = false)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var tomonatsu = new Card { Id = "hida-tomonatsu", Type = CardType.Character, Controller = p1 };
        var attacker = new Card { Id = "opponent-attacker", Type = CardType.Character, Controller = p2, Unique = attackerUnique };
        p1.PlayArea.Add(tomonatsu);
        p2.PlayArea.Add(attacker);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Winner = p1 };
        conflict.Defenders.Add(tomonatsu);
        conflict.Attackers.Add(attacker);
        game.CurrentConflict = conflict;

        return (game, tomonatsu, attacker);
    }

    [Test]
    public void SacrificesItselfToReturnANonUniqueAttackerToTheTopOfItsDeck()
    {
        var (game, tomonatsu, attacker) = NewGameWonWhileDefending();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = tomonatsu, Target = attacker };

        new HidaTomonatsuReturnAttackerToDeckOnDefendedWin().Execute(context);

        Assert.That(game.Player1.Discard, Does.Contain(tomonatsu), "sacrificed as the cost");
        Assert.That(game.Player2.Deck, Does.Contain(attacker));
        Assert.That(game.Player2.Deck[0], Is.EqualTo(attacker), "returned to the top");
        Assert.That(game.Player2.PlayArea, Does.Not.Contain(attacker));
    }

    [Test]
    public void CannotReturnAUniqueAttacker()
    {
        var (game, tomonatsu, attacker) = NewGameWonWhileDefending(attackerUnique: true);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = tomonatsu, Target = attacker };

        Assert.Throws<InvalidOperationException>(() => new HidaTomonatsuReturnAttackerToDeckOnDefendedWin().Execute(context));
        Assert.That(game.Player1.PlayArea, Does.Contain(tomonatsu), "not sacrificed - the cost was never paid");
    }

    [Test]
    public void WhenTheControllerDoesNotWin_Throws()
    {
        var (game, tomonatsu, attacker) = NewGameWonWhileDefending();
        game.CurrentConflict!.Winner = game.Player2;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = tomonatsu, Target = attacker };

        Assert.Throws<InvalidOperationException>(() => new HidaTomonatsuReturnAttackerToDeckOnDefendedWin().Execute(context));
    }

    [Test]
    public void WhileAttacking_Throws()
    {
        var (game, tomonatsu, attacker) = NewGameWonWhileDefending();
        game.CurrentConflict!.Defenders.Remove(tomonatsu);
        game.CurrentConflict!.Attackers.Add(tomonatsu);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = tomonatsu, Target = attacker };

        Assert.Throws<InvalidOperationException>(() => new HidaTomonatsuReturnAttackerToDeckOnDefendedWin().Execute(context));
    }
}
