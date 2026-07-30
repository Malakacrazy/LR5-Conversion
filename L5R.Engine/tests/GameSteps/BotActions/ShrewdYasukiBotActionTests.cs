using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class ShrewdYasukiBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card yasuki, out Card topCard)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        yasuki = new Card { Id = "shrewd-yasuki", Type = CardType.Character, Controller = p1 };
        var revealedHolding = new Card { Id = "revealed-holding", Type = CardType.Holding, Controller = p1, Facedown = false };
        p1.Provinces.Add(revealedHolding);
        p1.PlayArea.Add(yasuki);

        topCard = new Card { Id = "top-card", Type = CardType.Character, Controller = p1 };
        var secondCard = new Card { Id = "second-card", Type = CardType.Character, Controller = p1 };
        p1.Deck.Add(topCard);
        p1.Deck.Add(secondCard);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(yasuki);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WhileParticipatingWithARevealedHoldingAndCardsInDeck_True()
    {
        var game = NewScenario(out var p1, out var yasuki, out _);

        Assert.That(new ShrewdYasukiBotAction().IsLegal(game, yasuki, p1), Is.True);
    }

    [Test]
    public void IsLegal_WithNoRevealedHoldingInEitherPlayersProvinces_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var yasuki = new Card { Id = "shrewd-yasuki", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(yasuki);
        p1.Deck.Add(new Card { Id = "top-card", Type = CardType.Character, Controller = p1 });
        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(yasuki);
        game.CurrentConflict = conflict;

        Assert.That(new ShrewdYasukiBotAction().IsLegal(game, yasuki, p1), Is.False);
    }

    [Test]
    public void Invoke_KeepsTheTopCardAndBottomsTheOther()
    {
        var game = NewScenario(out var p1, out var yasuki, out var topCard);

        new ShrewdYasukiBotAction().Invoke(game, yasuki, p1);

        Assert.That(p1.Hand, Contains.Item(topCard));
        Assert.That(p1.Deck, Has.Count.EqualTo(1));
        Assert.That(p1.Deck[0].Id, Is.EqualTo("second-card"));
    }
}
