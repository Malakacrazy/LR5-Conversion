using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class WayOfTheUnicornTests
{
    [Test]
    public void CurrentFirstPlayer_KeepsTheTokenAcrossTheRoundRollover()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Fate };
        var way = new Card { Id = "way-of-the-unicorn", Type = CardType.Event, Controller = p1 };

        var context = new AbilityContext { Game = game, Player = p1, Source = way };
        new WayOfTheUnicornKeepFirstPlayerToken().Execute(context);

        game.AdvancePhase(); // -> Dynasty (round 2)

        Assert.That(game.ActivePlayer, Is.EqualTo(p1));
    }

    [Test]
    public void WithoutBeingPlayed_TheTokenPassesNormally()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Fate };

        game.AdvancePhase(); // -> Dynasty (round 2)

        Assert.That(game.ActivePlayer, Is.EqualTo(p2));
    }

    [Test]
    public void PlayedByThePlayerWhoDoesNotCurrentlyHoldTheToken_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Fate };
        var way = new Card { Id = "way-of-the-unicorn", Type = CardType.Event, Controller = p2 };

        var context = new AbilityContext { Game = game, Player = p2, Source = way };

        Assert.Throws<InvalidOperationException>(() => new WayOfTheUnicornKeepFirstPlayerToken().Execute(context));
    }
}
