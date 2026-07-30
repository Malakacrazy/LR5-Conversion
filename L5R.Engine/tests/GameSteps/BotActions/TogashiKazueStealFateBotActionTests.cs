using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class TogashiKazueStealFateBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card kazue, out Card parent, out Card opponentWithFate)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        parent = new Card { Id = "parent", Type = CardType.Character, Controller = p1, Fate = 0 };
        kazue = new Card { Id = "togashi-kazue", Type = CardType.Character, Controller = p1, AttachedTo = parent };
        opponentWithFate = new Card { Id = "opponent-with-fate", Type = CardType.Character, Controller = p2, Fate = 3 };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(kazue);
        p2.PlayArea.Add(opponentWithFate);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(parent);
        conflict.Defenders.Add(opponentWithFate);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WithAParticipantWithFateToSteal_True()
    {
        var game = NewScenario(out var p1, out var kazue, out _, out _);

        Assert.That(new TogashiKazueStealFateBotAction().IsLegal(game, kazue, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenTheOnlyOtherParticipantHasNoFate_False()
    {
        var game = NewScenario(out var p1, out var kazue, out _, out var opponentWithFate);
        opponentWithFate.Fate = 0;

        Assert.That(new TogashiKazueStealFateBotAction().IsLegal(game, kazue, p1), Is.False);
    }

    [Test]
    public void Invoke_StealsFateAndGivesItToTheParent()
    {
        var game = NewScenario(out var p1, out var kazue, out var parent, out var opponentWithFate);

        new TogashiKazueStealFateBotAction().Invoke(game, kazue, p1);

        Assert.That(opponentWithFate.Fate, Is.EqualTo(2));
        Assert.That(parent.Fate, Is.EqualTo(1));
    }
}
