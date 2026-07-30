using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class IndomitableWillBotActionTests
{
    private static (GameState game, Card iw, Card solo) NewScenario()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var iw = new Card { Id = "indomitable-will", Type = CardType.Event, Controller = p1 };
        var solo = new Card { Id = "solo-participant", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(solo);
        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Winner = p1 };
        conflict.Attackers.Add(solo);
        game.CurrentConflict = conflict;
        return (game, iw, solo);
    }

    [Test]
    public void IsLegal_WithExactlyOneParticipantAndAWin_True()
    {
        var (game, iw, _) = NewScenario();
        Assert.That(new IndomitableWillBotAction().IsLegal(game, iw, game.Player1), Is.True);
    }

    [Test]
    public void IsLegal_WithTwoParticipants_False()
    {
        var (game, iw, _) = NewScenario();
        var second = new Card { Id = "second", Type = CardType.Character, Controller = game.Player1 };
        game.Player1.PlayArea.Add(second);
        game.CurrentConflict!.Attackers.Add(second);

        Assert.That(new IndomitableWillBotAction().IsLegal(game, iw, game.Player1), Is.False);
    }

    [Test]
    public void Invoke_PreventsTheSoloParticipantFromBowing()
    {
        var (game, iw, solo) = NewScenario();

        new IndomitableWillBotAction().Invoke(game, iw, game.Player1);

        Assert.That(game.IsRestrictedFrom(solo, "bow"), Is.True);
    }
}
