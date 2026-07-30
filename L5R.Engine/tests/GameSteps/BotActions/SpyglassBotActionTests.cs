using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class SpyglassBotActionTests
{
    [Test]
    public void IsLegal_WhileTheParentIsParticipating_True()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var parent = new Card { Id = "parent", Type = CardType.Character, Controller = p1 };
        var spyglass = new Card { Id = "spyglass", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(spyglass);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(parent);
        game.CurrentConflict = conflict;

        Assert.That(new SpyglassBotAction().IsLegal(game, spyglass, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenTheParentIsNotParticipating_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var parent = new Card { Id = "parent", Type = CardType.Character, Controller = p1 };
        var spyglass = new Card { Id = "spyglass", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(spyglass);

        Assert.That(new SpyglassBotAction().IsLegal(game, spyglass, p1), Is.False);
    }

    [Test]
    public void Invoke_DrawsACard()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var parent = new Card { Id = "parent", Type = CardType.Character, Controller = p1 };
        var spyglass = new Card { Id = "spyglass", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(spyglass);
        p1.Deck.Add(new Card { Id = "top-card", Type = CardType.Character, Controller = p1 });

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(parent);
        game.CurrentConflict = conflict;

        new SpyglassBotAction().Invoke(game, spyglass, p1);

        Assert.That(p1.Hand, Has.Count.EqualTo(1));
    }
}
