using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class KakitaAsamiBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card asami)
    {
        p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        asami = new Card { Id = "kakita-asami", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(asami);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political", AttackerSkill = 5, DefenderSkill = 2 };
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_InPlayWhenItsControllersSideHasMoreSkill_True()
    {
        var game = NewScenario(out var p1, out var asami);

        Assert.That(new KakitaAsamiBotAction().IsLegal(game, asami, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenSittingUnplayedInHand_False()
    {
        var game = NewScenario(out var p1, out var asami);
        p1.PlayArea.Remove(asami);
        asami.Location = "hand";
        p1.Hand.Add(asami);

        Assert.That(new KakitaAsamiBotAction().IsLegal(game, asami, p1), Is.False);
    }

    [Test]
    public void Invoke_TakesOneHonorFromTheOpponent()
    {
        var game = NewScenario(out var p1, out var asami);

        new KakitaAsamiBotAction().Invoke(game, asami, p1);

        Assert.That(p1.Honor, Is.EqualTo(6));
        Assert.That(game.Player2.Honor, Is.EqualTo(4));
    }
}
