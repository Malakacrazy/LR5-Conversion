using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class NitenAdeptBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card nitenAdept, out Card ownAttachment, out Card unattachedOpponent)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        nitenAdept = new Card { Id = "niten-adept", Type = CardType.Character, Controller = p1 };
        ownAttachment = new Card { Id = "own-attachment", Type = CardType.Attachment, Controller = p1, AttachedTo = nitenAdept };
        unattachedOpponent = new Card { Id = "unattached-opponent", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(nitenAdept);
        p1.PlayArea.Add(ownAttachment);
        p2.PlayArea.Add(unattachedOpponent);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(nitenAdept);
        conflict.Defenders.Add(unattachedOpponent);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WithAnUnbowedOwnAttachmentAndAnUnattachedParticipant_True()
    {
        var game = NewScenario(out var p1, out var nitenAdept, out _, out _);

        Assert.That(new NitenAdeptBotAction().IsLegal(game, nitenAdept, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenTheTargetHasAnAttachment_False()
    {
        var game = NewScenario(out var p1, out var nitenAdept, out _, out var unattachedOpponent);
        var p2 = game.Player2;
        p2.PlayArea.Add(new Card { Id = "opponent-attachment", Type = CardType.Attachment, Controller = p2, AttachedTo = unattachedOpponent });

        Assert.That(new NitenAdeptBotAction().IsLegal(game, nitenAdept, p1), Is.False);
    }

    [Test]
    public void Invoke_BowsItsOwnAttachmentAndTheUnattachedParticipant()
    {
        var game = NewScenario(out var p1, out var nitenAdept, out var ownAttachment, out var unattachedOpponent);

        new NitenAdeptBotAction().Invoke(game, nitenAdept, p1);

        Assert.That(ownAttachment.Bowed, Is.True);
        Assert.That(unattachedOpponent.Bowed, Is.True);
    }
}
