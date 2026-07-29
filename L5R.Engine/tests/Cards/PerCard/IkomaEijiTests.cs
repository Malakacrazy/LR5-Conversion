using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class IkomaEijiTests
{
    [Test]
    public void AfterLosingAPoliticalConflict_PutsACheapBushiIntoPlayFromProvinces()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var eiji = new Card { Id = "ikoma-eiji", Type = CardType.Character, Controller = p1 };
        var bushi = new Card { Id = "cheap-bushi", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" }, PrintedCost = 3 };
        p1.PlayArea.Add(eiji);
        p1.Provinces.Add(bushi);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "political", Loser = p1 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = eiji, Target = bushi };

        new IkomaEijiPutBushiIntoPlayOnPoliticalLoss().Execute(context);

        Assert.That(p1.PlayArea, Does.Contain(bushi));
        Assert.That(p1.Provinces, Does.Not.Contain(bushi));
    }

    [Test]
    public void WithACostFourOrHigherBushi_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var eiji = new Card { Id = "ikoma-eiji", Type = CardType.Character, Controller = p1 };
        var expensiveBushi = new Card { Id = "expensive-bushi", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" }, PrintedCost = 4 };
        p1.PlayArea.Add(eiji);
        p1.Provinces.Add(expensiveBushi);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "political", Loser = p1 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = eiji, Target = expensiveBushi };

        Assert.Throws<InvalidOperationException>(() => new IkomaEijiPutBushiIntoPlayOnPoliticalLoss().Execute(context));
    }

    [Test]
    public void WhenWinningTheConflict_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var eiji = new Card { Id = "ikoma-eiji", Type = CardType.Character, Controller = p1 };
        var bushi = new Card { Id = "cheap-bushi", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" }, PrintedCost = 3 };
        p1.PlayArea.Add(eiji);
        p1.Provinces.Add(bushi);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "political", Loser = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = eiji, Target = bushi };

        Assert.Throws<InvalidOperationException>(() => new IkomaEijiPutBushiIntoPlayOnPoliticalLoss().Execute(context));
    }
}
