using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.Costs;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class ReadyForBattleFirerTests
{
    private static (Player p1, Player p2, GameState game, Card target) NewScenario()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var target = new Card { Id = "my-character", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(target);
        var readyForBattle = new Card { Id = "ready-for-battle", Type = CardType.Event, Controller = p1, Location = "hand", PrintedCost = 0 };
        p1.Hand.Add(readyForBattle);
        return (p1, p2, game, target);
    }

    [Test]
    public void FireIfLegal_WhenTheOpponentCausesTheBow_ReadiesTheTargetAndDiscardsItself()
    {
        var (p1, p2, game, target) = NewScenario();
        var readyForBattle = p1.Hand[0];

        ReadyForBattleFirer.FireIfLegal(game, p2, target);

        Assert.That(target.Bowed, Is.False);
        Assert.That(p1.Hand, Does.Not.Contain(readyForBattle));
        Assert.That(p1.Discard, Contains.Item(readyForBattle));
    }

    [Test]
    public void FireIfLegal_WhenTheTargetsOwnControllerCausesTheBow_DoesNotFire()
    {
        var (p1, _, game, target) = NewScenario();
        var readyForBattle = p1.Hand[0];
        target.Bowed = true;

        ReadyForBattleFirer.FireIfLegal(game, p1, target);

        Assert.That(target.Bowed, Is.True);
        Assert.That(p1.Hand, Contains.Item(readyForBattle));
    }

    [Test]
    public void FireIfLegal_WithoutReadyForBattleInHand_DoesNotThrow()
    {
        var (p1, p2, game, target) = NewScenario();
        p1.Hand.Clear();

        Assert.DoesNotThrow(() => ReadyForBattleFirer.FireIfLegal(game, p2, target));
    }

    [Test]
    public void BowGameActionHandler_BowingAnOpponentsCharacter_FiresReadyForBattle()
    {
        var (p1, p2, game, target) = NewScenario();
        var readyForBattle = p1.Hand[0];

        var context = new AbilityContext { Game = game, Player = p2, Source = target, Target = target };
        new BowGameActionHandler().Execute(context, null);

        Assert.That(target.Bowed, Is.False, "bowed then immediately readied back by ready-for-battle");
        Assert.That(p1.Discard, Contains.Item(readyForBattle));
    }

    [Test]
    public void BowSelfCostHandler_PayingABowCost_DoesNotTriggerReadyForBattle()
    {
        var (p1, _, game, target) = NewScenario();
        var readyForBattle = p1.Hand[0];

        var context = new AbilityContext { Game = game, Player = p1, Source = target };
        new BowSelfCostHandler().Pay(context, null);

        Assert.That(target.Bowed, Is.True, "BowSelfCostHandler doesn't route through BowGameActionHandler, so ready-for-battle never even gets a chance to fire");
        Assert.That(p1.Hand, Contains.Item(readyForBattle));
    }
}
