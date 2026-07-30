using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class VengefulOathkeeperBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card oathkeeper)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        oathkeeper = new Card { Id = "vengeful-oathkeeper", Type = CardType.Character, Controller = p1, Location = "hand" };
        p1.Hand.Add(oathkeeper);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military", Loser = p1 };
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_InHandAfterLosingAMilitaryConflict_True()
    {
        var game = NewScenario(out var p1, out var oathkeeper);

        Assert.That(new VengefulOathkeeperBotAction().IsLegal(game, oathkeeper, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenNotInHand_False()
    {
        var game = NewScenario(out var p1, out var oathkeeper);
        p1.Hand.Remove(oathkeeper);
        p1.PlayArea.Add(oathkeeper);

        Assert.That(new VengefulOathkeeperBotAction().IsLegal(game, oathkeeper, p1), Is.False);
    }

    [Test]
    public void Invoke_PutsItselfIntoPlayJoiningTheConflictAsAnAttacker()
    {
        var game = NewScenario(out var p1, out var oathkeeper);

        new VengefulOathkeeperBotAction().Invoke(game, oathkeeper, p1);

        Assert.That(p1.PlayArea, Contains.Item(oathkeeper));
        Assert.That(game.CurrentConflict!.Attackers, Contains.Item(oathkeeper));
    }
}
