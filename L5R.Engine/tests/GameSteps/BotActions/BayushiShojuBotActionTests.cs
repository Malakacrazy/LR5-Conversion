using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class BayushiShojuBotActionTests
{
    private static GameState NewPoliticalConflict(out Player p1, out Player p2, out Card shoju, out Card opponentParticipant)
    {
        p1 = new Player { Name = "Player1" };
        p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        shoju = new Card { Id = "bayushi-shoju", Type = CardType.Character, Controller = p1, PrintedPoliticalSkill = 7 };
        opponentParticipant = new Card { Id = "opponent-participant", Type = CardType.Character, Controller = p2, PrintedPoliticalSkill = 3 };
        p1.PlayArea.Add(shoju);
        p2.PlayArea.Add(opponentParticipant);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        conflict.Attackers.Add(shoju);
        conflict.Defenders.Add(opponentParticipant);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_DuringAPoliticalConflictWithAnOpponentParticipant_True()
    {
        var game = NewPoliticalConflict(out var p1, out _, out var shoju, out _);

        Assert.That(new BayushiShojuBotAction().IsLegal(game, shoju, p1), Is.True);
    }

    [Test]
    public void IsLegal_DuringAMilitaryConflict_False()
    {
        var game = NewPoliticalConflict(out var p1, out _, out var shoju, out _);
        game.CurrentConflict!.ConflictType = "military";

        Assert.That(new BayushiShojuBotAction().IsLegal(game, shoju, p1), Is.False);
    }

    [Test]
    public void Invoke_ReducesTheOpponentsPoliticalSkillByOne()
    {
        var game = NewPoliticalConflict(out var p1, out _, out var shoju, out var opponentParticipant);

        new BayushiShojuBotAction().Invoke(game, shoju, p1);

        Assert.That(game.EffectivePoliticalSkill(opponentParticipant), Is.EqualTo(2));
    }

    [Test]
    public void Invoke_DiscardsTheTarget_WhenItsPoliticalSkillDropsBelowOne()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var shoju = new Card { Id = "bayushi-shoju", Type = CardType.Character, Controller = p1, PrintedPoliticalSkill = 7 };
        var frailOpponent = new Card { Id = "frail-opponent", Type = CardType.Character, Controller = p2, PrintedPoliticalSkill = 0 };
        p1.PlayArea.Add(shoju);
        p2.PlayArea.Add(frailOpponent);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        conflict.Attackers.Add(shoju);
        conflict.Defenders.Add(frailOpponent);
        game.CurrentConflict = conflict;

        new BayushiShojuBotAction().Invoke(game, shoju, p1);

        Assert.That(p2.PlayArea, Does.Not.Contain(frailOpponent));
        Assert.That(p2.Discard, Contains.Item(frailOpponent));
    }
}
