using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class JadeTetsuboBotActionTests
{
    private static GameState NewConflict(out Player p1, out Player p2, out Card parent, out Card tetsubo, out Card weakOpponent)
    {
        p1 = new Player { Name = "Player1" };
        p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        parent = new Card { Id = "parent", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 5 };
        tetsubo = new Card { Id = "jade-tetsubo", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        weakOpponent = new Card { Id = "weak-opponent", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 1, Fate = 2 };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(tetsubo);
        p2.PlayArea.Add(weakOpponent);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(parent);
        conflict.Defenders.Add(weakOpponent);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WhileParticipatingUnbowedWithALowerSkillParticipant_True()
    {
        var game = NewConflict(out var p1, out _, out _, out var tetsubo, out _);

        Assert.That(new JadeTetsuboBotAction().IsLegal(game, tetsubo, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenAlreadyBowed_False()
    {
        var game = NewConflict(out var p1, out _, out _, out var tetsubo, out _);
        tetsubo.Bowed = true;

        Assert.That(new JadeTetsuboBotAction().IsLegal(game, tetsubo, p1), Is.False);
    }

    [Test]
    public void Invoke_BowsItselfAndReturnsTheTargetsFateToItsController()
    {
        var game = NewConflict(out var p1, out var p2, out _, out var tetsubo, out var weakOpponent);

        new JadeTetsuboBotAction().Invoke(game, tetsubo, p1);

        Assert.That(tetsubo.Bowed, Is.True);
        Assert.That(weakOpponent.Fate, Is.EqualTo(0));
        Assert.That(p2.Fate, Is.EqualTo(2));
    }
}
