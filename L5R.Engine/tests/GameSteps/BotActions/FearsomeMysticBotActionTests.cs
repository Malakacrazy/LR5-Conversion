using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class FearsomeMysticBotActionTests
{
    [Test]
    public void IsLegal_WhileParticipating_True()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var mystic = new Card { Id = "fearsome-mystic", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(mystic);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(mystic);
        game.CurrentConflict = conflict;

        Assert.That(new FearsomeMysticBotAction().IsLegal(game, mystic, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhileNotParticipating_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var mystic = new Card { Id = "fearsome-mystic", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(mystic);

        Assert.That(new FearsomeMysticBotAction().IsLegal(game, mystic, p1), Is.False, "no CurrentConflict at all");
    }

    [Test]
    public void Invoke_RemovesFateFromLowerGloryOpponents()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var mystic = new Card { Id = "fearsome-mystic", Type = CardType.Character, Controller = p1, PrintedGlory = 3 };
        var weakerOpponent = new Card { Id = "weaker", Type = CardType.Character, Controller = p2, PrintedGlory = 1, Fate = 2 };
        p1.PlayArea.Add(mystic);
        p2.PlayArea.Add(weakerOpponent);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(mystic);
        conflict.Defenders.Add(weakerOpponent);
        game.CurrentConflict = conflict;

        new FearsomeMysticBotAction().Invoke(game, mystic, p1);

        Assert.That(weakerOpponent.Fate, Is.EqualTo(1));
    }
}
