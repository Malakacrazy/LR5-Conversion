using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class EnlightenedWarriorBotActionTests
{
    private static GameState NewScenario(out Player p1, out Player p2, out Card warrior, bool actingPlayerIsAttacker = false)
    {
        p1 = new Player { Name = "Player1" };
        p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        game.Rings[0].Fate = 2;

        warrior = new Card { Id = "enlightened-warrior", Type = CardType.Character, Controller = p1, Fate = 0 };
        p1.PlayArea.Add(warrior);

        var conflict = actingPlayerIsAttacker
            ? new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 }
            : new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Elements.Add(game.Rings[0].Element);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WhenOpponentAttacksARingWithFateOnIt_True()
    {
        var game = NewScenario(out var p1, out _, out var warrior);

        Assert.That(new EnlightenedWarriorBotAction().IsLegal(game, warrior, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenActingPlayerIsTheAttacker_False()
    {
        var game = NewScenario(out var p1, out _, out var warrior, actingPlayerIsAttacker: true);

        Assert.That(new EnlightenedWarriorBotAction().IsLegal(game, warrior, p1), Is.False);
    }

    [Test]
    public void Invoke_PlacesTheRingsFateOnItself()
    {
        var game = NewScenario(out var p1, out _, out var warrior);

        new EnlightenedWarriorBotAction().Invoke(game, warrior, p1);

        Assert.That(warrior.Fate, Is.EqualTo(1));
    }
}
