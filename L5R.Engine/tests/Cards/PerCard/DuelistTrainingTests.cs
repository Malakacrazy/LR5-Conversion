using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class DuelistTrainingTests
{
    private static (GameState Game, Card Host, Card Target) NewGameBothParticipating()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var host = new Card { Id = "host-character", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "enemy-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(host);
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(host);
        conflict.Defenders.Add(target);
        game.CurrentConflict = conflict;

        return (game, host, target);
    }

    [Test]
    public void WithEqualBids_BowsTheLoserWithNoCost()
    {
        var (game, host, target) = NewGameBothParticipating();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = host, Target = target, DuelWinner = host };

        new DuelistTrainingGrantMilitaryDuelAction().Execute(context);

        Assert.That(target.Bowed, Is.True);
        Assert.That(host.Bowed, Is.False);
    }

    [Test]
    public void WhenTheLowBidderPaysWithHonor_TransfersTheDifference()
    {
        var (game, host, target) = NewGameBothParticipating();
        game.Player1.ShowBid = 2;
        game.Player2.ShowBid = 5;
        game.Player1.Honor = 3;
        game.Player2.Honor = 3;

        var context = new AbilityContext
        {
            Game = game, Player = game.Player1, Source = host, Target = target,
            ChosenChoice = "Pay with honor", DuelWinner = host
        };

        new DuelistTrainingGrantMilitaryDuelAction().Execute(context);

        Assert.That(game.Player1.Honor, Is.EqualTo(0), "the low bidder pays the 3-point difference");
        Assert.That(game.Player2.Honor, Is.EqualTo(6));
    }

    [Test]
    public void WhenTheLowBidderPaysWithCards_DiscardsFromTheirOwnHand()
    {
        var (game, host, target) = NewGameBothParticipating();
        game.Player1.ShowBid = 1;
        game.Player2.ShowBid = 4;
        var discarded1 = new Card { Id = "discard-1", Type = CardType.Character, Controller = game.Player1, Location = "hand" };
        var discarded2 = new Card { Id = "discard-2", Type = CardType.Character, Controller = game.Player1, Location = "hand" };
        var discarded3 = new Card { Id = "discard-3", Type = CardType.Character, Controller = game.Player1, Location = "hand" };
        game.Player1.Hand.Add(discarded1);
        game.Player1.Hand.Add(discarded2);
        game.Player1.Hand.Add(discarded3);

        var context = new AbilityContext
        {
            Game = game, Player = game.Player1, Source = host, Target = target,
            ChosenChoice = "Pay with cards", ChosenDiscardCards = new[] { discarded1, discarded2, discarded3 },
            DuelWinner = host
        };

        new DuelistTrainingGrantMilitaryDuelAction().Execute(context);

        Assert.That(game.Player1.Discard, Does.Contain(discarded1));
        Assert.That(game.Player1.Discard, Does.Contain(discarded2));
        Assert.That(game.Player1.Discard, Does.Contain(discarded3));
    }

    [Test]
    public void WhenHostLosesTheDuel_HostBowsInstead()
    {
        var (game, host, target) = NewGameBothParticipating();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = host, Target = target, DuelWinner = target };

        new DuelistTrainingGrantMilitaryDuelAction().Execute(context);

        Assert.That(host.Bowed, Is.True);
        Assert.That(target.Bowed, Is.False);
    }

    [Test]
    public void WithABidDifferenceAndNoChosenPaymentMethod_Throws()
    {
        var (game, host, target) = NewGameBothParticipating();
        game.Player1.ShowBid = 1;
        game.Player2.ShowBid = 4;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = host, Target = target, DuelWinner = host };

        Assert.Throws<InvalidOperationException>(() => new DuelistTrainingGrantMilitaryDuelAction().Execute(context));
    }
}
