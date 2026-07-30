using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class BanzaiBotActionTests
{
    [Test]
    public void IsLegal_WithAnOwnParticipant_True()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var banzai = new Card { Id = "banzai", Type = CardType.Event, Controller = p1 };
        var participant = new Card { Id = "participant", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 3 };
        p1.PlayArea.Add(participant);
        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(participant);
        game.CurrentConflict = conflict;

        Assert.That(new BanzaiBotAction().IsLegal(game, banzai, p1), Is.True);
    }

    [Test]
    public void IsLegal_WithNoActiveConflict_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var banzai = new Card { Id = "banzai", Type = CardType.Event, Controller = p1 };

        Assert.That(new BanzaiBotAction().IsLegal(game, banzai, p1), Is.False);
    }

    [Test]
    public void Invoke_GrantsPlusTwoMilitarySkill()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var banzai = new Card { Id = "banzai", Type = CardType.Event, Controller = p1 };
        var participant = new Card { Id = "participant", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 3 };
        p1.PlayArea.Add(participant);
        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(participant);
        game.CurrentConflict = conflict;

        new BanzaiBotAction().Invoke(game, banzai, p1);

        Assert.That(game.EffectiveMilitarySkill(participant), Is.EqualTo(5));
    }
}
