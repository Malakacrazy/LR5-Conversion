using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class GiverOfGiftsBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card giver, out Card attachment, out Card oldParent, out Card newParent)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        giver = new Card { Id = "giver-of-gifts", Type = CardType.Character, Controller = p1 };
        oldParent = new Card { Id = "old-parent", Type = CardType.Character, Controller = p1 };
        newParent = new Card { Id = "new-parent", Type = CardType.Character, Controller = p1 };
        attachment = new Card { Id = "some-attachment", Type = CardType.Attachment, Controller = p1, AttachedTo = oldParent };
        p1.PlayArea.Add(giver);
        p1.PlayArea.Add(oldParent);
        p1.PlayArea.Add(newParent);
        p1.PlayArea.Add(attachment);

        return game;
    }

    [Test]
    public void IsLegal_WithAnAttachmentAndAnotherCharacterToMoveItTo_True()
    {
        var game = NewScenario(out var p1, out var giver, out _, out _, out _);

        Assert.That(new GiverOfGiftsBotAction().IsLegal(game, giver, p1), Is.True);
    }

    [Test]
    public void Invoke_MovesTheAttachmentAwayFromItsOldParent()
    {
        // The adapter picks the first legal destination character it controls, which may be
        // giver-of-gifts itself (a legal move per the script's own rules - it doesn't
        // exclude the source card as a destination) rather than any specific other character,
        // so this only asserts the one guaranteed outcome: it moved.
        var game = NewScenario(out var p1, out var giver, out var attachment, out var oldParent, out _);

        new GiverOfGiftsBotAction().Invoke(game, giver, p1);

        Assert.That(attachment.AttachedTo, Is.Not.EqualTo(oldParent));
    }
}
