using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ShrewdYasukiTests
{
    private static (GameState Game, Card Yasuki, Card First, Card Second) NewGameParticipatingWithRevealedHolding()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var yasuki = new Card { Id = "shrewd-yasuki", Type = CardType.Character, Controller = p1 };
        var holding = new Card { Id = "some-holding", Type = CardType.Holding, Controller = p1, Facedown = false };
        p1.PlayArea.Add(yasuki);
        p1.Provinces.Add(holding);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(yasuki);
        game.CurrentConflict = conflict;

        var first = new Card { Id = "first-card", Type = CardType.Character, Controller = p1 };
        var second = new Card { Id = "second-card", Type = CardType.Character, Controller = p1 };
        p1.Deck.Add(first);
        p1.Deck.Add(second);

        return (game, yasuki, first, second);
    }

    [Test]
    public void TakesTheChosenCardAndBottomsTheOther()
    {
        var (game, yasuki, first, second) = NewGameParticipatingWithRevealedHolding();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = yasuki, ChosenDeckSearchCard = first };

        new ShrewdYasukiLookAtTopTwoKeepOne().Execute(context);

        Assert.That(game.Player1.Hand, Does.Contain(first));
        Assert.That(game.Player1.Deck, Does.Contain(second));
        Assert.That(game.Player1.Deck[^1], Is.EqualTo(second), "the unchosen card goes to the bottom");
    }

    [Test]
    public void WithNoRevealedHoldingInEitherPlayersProvinces_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var yasuki = new Card { Id = "shrewd-yasuki", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(yasuki);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(yasuki);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = yasuki, ChosenDeckSearchCard = null };

        Assert.Throws<InvalidOperationException>(() => new ShrewdYasukiLookAtTopTwoKeepOne().Execute(context));
    }

    [Test]
    public void WhenNotParticipating_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var yasuki = new Card { Id = "shrewd-yasuki", Type = CardType.Character, Controller = p1 };
        var holding = new Card { Id = "some-holding", Type = CardType.Holding, Controller = p1, Facedown = false };
        p1.PlayArea.Add(yasuki);
        p1.Provinces.Add(holding);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = yasuki };

        Assert.Throws<InvalidOperationException>(() => new ShrewdYasukiLookAtTopTwoKeepOne().Execute(context));
    }
}
