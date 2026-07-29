using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class IdeTraderTests
{
    private static (GameState Game, Card Trader) NewGameParticipating()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var trader = new Card { Id = "ide-trader", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(trader);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(trader);
        game.CurrentConflict = conflict;

        return (game, trader);
    }

    [Test]
    public void ChoosingFate_GainsOneFate()
    {
        var (game, trader) = NewGameParticipating();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = trader, ChosenChoice = "Gain 1 fate" };

        new IdeTraderGainFateOrDrawOnAllyMovingToConflict().Execute(context);

        Assert.That(game.Player1.Fate, Is.EqualTo(1));
    }

    [Test]
    public void ChoosingDraw_DrawsOneCard()
    {
        var (game, trader) = NewGameParticipating();
        game.Player1.Deck.Add(new Card { Id = "some-card", Type = CardType.Character, Controller = game.Player1 });
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = trader, ChosenChoice = "Draw 1 card" };

        new IdeTraderGainFateOrDrawOnAllyMovingToConflict().Execute(context);

        Assert.That(game.Player1.Hand, Has.Count.EqualTo(1));
    }

    [Test]
    public void WhenNotParticipating_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var trader = new Card { Id = "ide-trader", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(trader);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = trader, ChosenChoice = "Gain 1 fate" };

        Assert.Throws<InvalidOperationException>(() => new IdeTraderGainFateOrDrawOnAllyMovingToConflict().Execute(context));
    }
}
