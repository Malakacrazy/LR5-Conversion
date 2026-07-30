using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class SteadfastSamuraiFirerTests
{
    [Test]
    public void FireIfLegal_WhenAheadByFiveHonor_ProtectsFromDiscardAndFateRemoval()
    {
        var p1 = new Player { Name = "Player1", Honor = 10 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Fate };
        var samurai = new Card { Id = "steadfast-samurai", Type = CardType.Character, Controller = p1, Fate = 2 };
        p1.PlayArea.Add(samurai);

        SteadfastSamuraiFirer.FireIfLegal(game, p1);

        Assert.Throws<InvalidOperationException>(
            () => new RemoveFateGameActionHandler().Execute(new AbilityContext { Game = game, Player = p1, Source = samurai, Target = samurai }, null));
        Assert.Throws<InvalidOperationException>(
            () => new DiscardFromPlayGameActionHandler().Execute(new AbilityContext { Game = game, Player = p1, Source = samurai, Target = samurai }, null));
    }

    [Test]
    public void FireIfLegal_WhenNotAheadByFiveHonor_DoesNotProtect()
    {
        var p1 = new Player { Name = "Player1", Honor = 8 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Fate };
        var samurai = new Card { Id = "steadfast-samurai", Type = CardType.Character, Controller = p1, Fate = 2 };
        p1.PlayArea.Add(samurai);

        SteadfastSamuraiFirer.FireIfLegal(game, p1);

        Assert.That(game.IsRestrictedFrom(samurai, "removeFate"), Is.False);
    }

    [Test]
    public void FireIfLegal_WhenAtZeroFate_StillBlocksDiscardFromPlay()
    {
        var p1 = new Player { Name = "Player1", Honor = 10 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Fate };
        var samurai = new Card { Id = "steadfast-samurai", Type = CardType.Character, Controller = p1, Fate = 0 };
        p1.PlayArea.Add(samurai);

        SteadfastSamuraiFirer.FireIfLegal(game, p1);

        Assert.That(game.IsRestrictedFrom(samurai, "discardFromPlay"), Is.True);
        Assert.That(p1.PlayArea, Does.Contain(samurai));
    }
}
