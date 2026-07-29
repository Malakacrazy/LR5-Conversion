using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SteadfastSamuraiTests
{
    [Test]
    public void WhenAheadByFiveHonorAtFatePhase_CannotBeDiscardedOrLoseFate()
    {
        var p1 = new Player { Name = "Player1", Honor = 10 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Fate };
        var samurai = new Card { Id = "steadfast-samurai", Type = CardType.Character, Controller = p1, Fate = 2 };
        p1.PlayArea.Add(samurai);

        var context = new AbilityContext { Game = game, Player = p1, Source = samurai };

        new SteadfastSamuraiHonorThresholdProtection().Execute(context);

        Assert.Throws<InvalidOperationException>(
            () => new RemoveFateGameActionHandler().Execute(new AbilityContext { Game = game, Player = p1, Source = samurai, Target = samurai }, null));
        Assert.Throws<InvalidOperationException>(
            () => new DiscardFromPlayGameActionHandler().Execute(new AbilityContext { Game = game, Player = p1, Source = samurai, Target = samurai }, null));
    }

    [Test]
    public void WhenNotAheadByFiveHonor_Throws()
    {
        var p1 = new Player { Name = "Player1", Honor = 8 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Fate };
        var samurai = new Card { Id = "steadfast-samurai", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(samurai);

        var context = new AbilityContext { Game = game, Player = p1, Source = samurai };

        Assert.Throws<InvalidOperationException>(() => new SteadfastSamuraiHonorThresholdProtection().Execute(context));
    }

    [Test]
    public void OutsideTheFatePhase_Throws()
    {
        var p1 = new Player { Name = "Player1", Honor = 10 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var samurai = new Card { Id = "steadfast-samurai", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(samurai);

        var context = new AbilityContext { Game = game, Player = p1, Source = samurai };

        Assert.Throws<InvalidOperationException>(() => new SteadfastSamuraiHonorThresholdProtection().Execute(context));
    }
}
