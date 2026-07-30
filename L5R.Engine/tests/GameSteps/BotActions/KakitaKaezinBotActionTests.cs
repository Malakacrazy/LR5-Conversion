using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class KakitaKaezinBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card kaezin, out Card opponentParticipant, out Card uninvolvedAlly, int kaezinSkill = 5, int opponentSkill = 1)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        kaezin = new Card { Id = "kakita-kaezin", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = kaezinSkill };
        opponentParticipant = new Card { Id = "opponent-participant", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = opponentSkill };
        uninvolvedAlly = new Card { Id = "uninvolved-ally", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(kaezin);
        p1.PlayArea.Add(uninvolvedAlly);
        p2.PlayArea.Add(opponentParticipant);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(kaezin);
        conflict.Attackers.Add(uninvolvedAlly);
        conflict.Defenders.Add(opponentParticipant);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WhileParticipatingWithAnOpponentParticipant_True()
    {
        var game = NewScenario(out var p1, out var kaezin, out _, out _);

        Assert.That(new KakitaKaezinBotAction().IsLegal(game, kaezin, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenNotParticipating_False()
    {
        var game = NewScenario(out var p1, out var kaezin, out _, out _);
        game.CurrentConflict!.Attackers.Remove(kaezin);

        Assert.That(new KakitaKaezinBotAction().IsLegal(game, kaezin, p1), Is.False);
    }

    [Test]
    public void Invoke_WhenKaezinWins_SendsHomeTheUninvolvedParticipant()
    {
        var game = NewScenario(out var p1, out var kaezin, out var opponentParticipant, out var uninvolvedAlly, kaezinSkill: 5, opponentSkill: 1);

        new KakitaKaezinBotAction().Invoke(game, kaezin, p1);

        Assert.That(game.CurrentConflict!.Attackers, Does.Not.Contain(uninvolvedAlly));
        Assert.That(game.CurrentConflict!.Attackers, Contains.Item(kaezin));
        Assert.That(game.CurrentConflict!.Defenders, Contains.Item(opponentParticipant));
    }

    [Test]
    public void Invoke_WhenKaezinLoses_SendsHomeKaezin()
    {
        var game = NewScenario(out var p1, out var kaezin, out var opponentParticipant, out var uninvolvedAlly, kaezinSkill: 1, opponentSkill: 9);

        new KakitaKaezinBotAction().Invoke(game, kaezin, p1);

        Assert.That(game.CurrentConflict!.Attackers, Does.Not.Contain(kaezin));
        Assert.That(game.CurrentConflict!.Attackers, Contains.Item(uninvolvedAlly));
    }
}
