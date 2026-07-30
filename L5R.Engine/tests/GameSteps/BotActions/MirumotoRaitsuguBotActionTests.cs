using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class MirumotoRaitsuguBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card raitsugu, out Card opponentParticipant, int raitsuguSkill = 5, int opponentSkill = 1, int opponentFate = 2)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        raitsugu = new Card { Id = "mirumoto-raitsugu", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = raitsuguSkill };
        opponentParticipant = new Card { Id = "opponent-participant", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = opponentSkill, Fate = opponentFate };
        p1.PlayArea.Add(raitsugu);
        p2.PlayArea.Add(opponentParticipant);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(raitsugu);
        conflict.Defenders.Add(opponentParticipant);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WhileParticipatingWithAnOpponentParticipant_True()
    {
        var game = NewScenario(out var p1, out var raitsugu, out _);

        Assert.That(new MirumotoRaitsuguBotAction().IsLegal(game, raitsugu, p1), Is.True);
    }

    [Test]
    public void Invoke_WhenRaitsuguWins_RemovesFateFromTheLoser()
    {
        var game = NewScenario(out var p1, out var raitsugu, out var opponentParticipant, raitsuguSkill: 5, opponentSkill: 1, opponentFate: 2);

        new MirumotoRaitsuguBotAction().Invoke(game, raitsugu, p1);

        Assert.That(opponentParticipant.Fate, Is.EqualTo(1));
    }

    [Test]
    public void Invoke_WhenTheLoserHasNoFate_DiscardsItInstead()
    {
        var game = NewScenario(out var p1, out var raitsugu, out var opponentParticipant, raitsuguSkill: 5, opponentSkill: 1, opponentFate: 0);

        new MirumotoRaitsuguBotAction().Invoke(game, raitsugu, p1);

        Assert.That(game.Player2.Discard, Contains.Item(opponentParticipant));
    }
}
