using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class GiverOfGiftsTests
{
    [Test]
    public void MovesAnAttachmentToAnotherFriendlyCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var giver = new Card { Id = "giver-of-gifts", Type = CardType.Character, Controller = p1 };
        var oldParent = new Card { Id = "old-parent", Type = CardType.Character, Controller = p1 };
        var newParent = new Card { Id = "new-parent", Type = CardType.Character, Controller = p1 };
        var attachment = new Card { Id = "some-attachment", Type = CardType.Attachment, Controller = p1, AttachedTo = oldParent };
        p1.PlayArea.Add(giver);
        p1.PlayArea.Add(oldParent);
        p1.PlayArea.Add(newParent);
        p1.PlayArea.Add(attachment);

        var context = new AbilityContext { Game = game, Player = p1, Source = giver, Target = attachment, PlayAttachTarget = newParent };

        new GiverOfGiftsMoveAttachment().Execute(context);

        Assert.That(attachment.AttachedTo, Is.EqualTo(newParent));
        Assert.That(attachment.Controller, Is.EqualTo(p1), "no takeControl - controller is unchanged");
    }

    [Test]
    public void MovingToItsOwnCurrentParent_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var giver = new Card { Id = "giver-of-gifts", Type = CardType.Character, Controller = p1 };
        var parent = new Card { Id = "parent", Type = CardType.Character, Controller = p1 };
        var attachment = new Card { Id = "some-attachment", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(giver);
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(attachment);

        var context = new AbilityContext { Game = game, Player = p1, Source = giver, Target = attachment, PlayAttachTarget = parent };

        Assert.Throws<InvalidOperationException>(() => new GiverOfGiftsMoveAttachment().Execute(context));
    }

    [Test]
    public void MovingToAnOpponentsCharacter_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var giver = new Card { Id = "giver-of-gifts", Type = CardType.Character, Controller = p1 };
        var oldParent = new Card { Id = "old-parent", Type = CardType.Character, Controller = p1 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        var attachment = new Card { Id = "some-attachment", Type = CardType.Attachment, Controller = p1, AttachedTo = oldParent };
        p1.PlayArea.Add(giver);
        p1.PlayArea.Add(oldParent);
        p1.PlayArea.Add(attachment);
        p2.PlayArea.Add(opponentCharacter);

        var context = new AbilityContext { Game = game, Player = p1, Source = giver, Target = attachment, PlayAttachTarget = opponentCharacter };

        Assert.Throws<InvalidOperationException>(() => new GiverOfGiftsMoveAttachment().Execute(context));
    }
}
