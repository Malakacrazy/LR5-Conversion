using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class NitenAdeptTests
{
    private static (GameState Game, Card NitenAdept, Card OwnAttachment, Card UnattachedTarget) NewGameParticipatingWithAnAttachment()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var nitenAdept = new Card { Id = "niten-adept", Type = CardType.Character, Controller = p1 };
        var ownAttachment = new Card { Id = "own-attachment", Type = CardType.Attachment, Controller = p1, AttachedTo = nitenAdept };
        var unattachedTarget = new Card { Id = "unattached-target", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(nitenAdept);
        p1.PlayArea.Add(ownAttachment);
        p2.PlayArea.Add(unattachedTarget);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(nitenAdept);
        conflict.Defenders.Add(unattachedTarget);
        game.CurrentConflict = conflict;

        return (game, nitenAdept, ownAttachment, unattachedTarget);
    }

    [Test]
    public void BowsItsOwnAttachmentToBowAnUnattachedParticipant()
    {
        var (game, nitenAdept, ownAttachment, unattachedTarget) = NewGameParticipatingWithAnAttachment();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = nitenAdept, CostTarget = ownAttachment, Target = unattachedTarget };

        new NitenAdeptBowAttachmentToBowUnattachedParticipant().Execute(context);

        Assert.That(ownAttachment.Bowed, Is.True);
        Assert.That(unattachedTarget.Bowed, Is.True);
    }

    [Test]
    public void WithoutAnyAttachments_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var nitenAdept = new Card { Id = "niten-adept", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(nitenAdept);

        var context = new AbilityContext { Game = game, Player = p1, Source = nitenAdept };

        Assert.Throws<InvalidOperationException>(() => new NitenAdeptBowAttachmentToBowUnattachedParticipant().Execute(context));
    }

    [Test]
    public void CannotTargetACharacterWithAnAttachment()
    {
        var (game, nitenAdept, ownAttachment, unattachedTarget) = NewGameParticipatingWithAnAttachment();
        var targetsAttachment = new Card { Id = "targets-attachment", Type = CardType.Attachment, Controller = game.Player2, AttachedTo = unattachedTarget };
        game.Player2.PlayArea.Add(targetsAttachment);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = nitenAdept, CostTarget = ownAttachment, Target = unattachedTarget };

        Assert.Throws<InvalidOperationException>(() => new NitenAdeptBowAttachmentToBowUnattachedParticipant().Execute(context));
        Assert.That(ownAttachment.Bowed, Is.False, "the cost was never paid");
    }

    [Test]
    public void CostTargetNotAttachedToSelf_Throws()
    {
        var (game, nitenAdept, _, unattachedTarget) = NewGameParticipatingWithAnAttachment();
        var otherAttachment = new Card { Id = "other-attachment", Type = CardType.Attachment, Controller = game.Player1 };
        game.Player1.PlayArea.Add(otherAttachment);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = nitenAdept, CostTarget = otherAttachment, Target = unattachedTarget };

        Assert.Throws<InvalidOperationException>(() => new NitenAdeptBowAttachmentToBowUnattachedParticipant().Execute(context));
    }
}
