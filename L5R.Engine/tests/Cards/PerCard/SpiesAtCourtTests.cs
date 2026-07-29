using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SpiesAtCourtTests
{
    private static (GameState Game, Card SpiesAtCourt, Card CostTarget) NewGameWonPoliticalConflict()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var spiesAtCourt = new Card { Id = "spies-at-court", Type = CardType.Character, Controller = p1 };
        var costTarget = new Card { Id = "dishonor-fodder", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(spiesAtCourt);
        p1.PlayArea.Add(costTarget);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political", Winner = p1 };
        conflict.Attackers.Add(spiesAtCourt);
        conflict.Attackers.Add(costTarget);
        game.CurrentConflict = conflict;

        return (game, spiesAtCourt, costTarget);
    }

    [Test]
    public void DishonorsTheCostTargetAndDiscardsTwoChosenCardsFromTheOpponentsHand()
    {
        var (game, spiesAtCourt, costTarget) = NewGameWonPoliticalConflict();
        var card1 = new Card { Id = "opponent-card-1", Type = CardType.Character, Controller = game.Player2, Location = "hand" };
        var card2 = new Card { Id = "opponent-card-2", Type = CardType.Character, Controller = game.Player2, Location = "hand" };
        game.Player2.Hand.Add(card1);
        game.Player2.Hand.Add(card2);

        var context = new AbilityContext
        {
            Game = game, Player = game.Player1, Source = spiesAtCourt, CostTarget = costTarget,
            ChosenDiscardCards = new[] { card1, card2 }
        };

        new SpiesAtCourtDiscardTwoOnPoliticalWin().Execute(context);

        Assert.That(costTarget.IsDishonored, Is.True);
        Assert.That(game.Player2.Discard, Does.Contain(card1));
        Assert.That(game.Player2.Discard, Does.Contain(card2));
    }

    [Test]
    public void WhenLosingTheConflict_Throws()
    {
        var (game, spiesAtCourt, costTarget) = NewGameWonPoliticalConflict();
        game.CurrentConflict!.Winner = game.Player2;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = spiesAtCourt, CostTarget = costTarget };

        Assert.Throws<InvalidOperationException>(() => new SpiesAtCourtDiscardTwoOnPoliticalWin().Execute(context));
    }

    [Test]
    public void WithAnAlreadyDishonoredCostTarget_Throws()
    {
        var (game, spiesAtCourt, costTarget) = NewGameWonPoliticalConflict();
        costTarget.IsDishonored = true;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = spiesAtCourt, CostTarget = costTarget };

        Assert.Throws<InvalidOperationException>(() => new SpiesAtCourtDiscardTwoOnPoliticalWin().Execute(context));
    }
}
