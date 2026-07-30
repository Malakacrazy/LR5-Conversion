using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class IkomaEijiBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card eiji, out Card bushi, int bushiCost = 2)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        eiji = new Card { Id = "ikoma-eiji", Type = CardType.Character, Controller = p1 };
        bushi = new Card { Id = "cheap-bushi", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" }, PrintedCost = bushiCost };
        p1.PlayArea.Add(eiji);
        p1.Discard.Add(bushi);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "political", Loser = p1 };
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_AfterLosingAPoliticalConflictWithACheapBushiInDiscard_True()
    {
        var game = NewScenario(out var p1, out var eiji, out _);

        Assert.That(new IkomaEijiBotAction().IsLegal(game, eiji, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenTheBushiCostsFourOrMore_False()
    {
        var game = NewScenario(out var p1, out var eiji, out _, bushiCost: 4);

        Assert.That(new IkomaEijiBotAction().IsLegal(game, eiji, p1), Is.False);
    }

    [Test]
    public void Invoke_PutsTheBushiIntoPlayFromDiscard()
    {
        var game = NewScenario(out var p1, out var eiji, out var bushi);

        new IkomaEijiBotAction().Invoke(game, eiji, p1);

        Assert.That(p1.Discard, Does.Not.Contain(bushi));
        Assert.That(p1.PlayArea, Contains.Item(bushi));
    }
}
