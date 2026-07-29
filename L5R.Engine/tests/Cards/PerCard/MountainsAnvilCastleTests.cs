using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// mountain-s-anvil-castle's own action is scriptOverride'd
/// (MountainsAnvilCastleBonusForAttachments) - its cardCondition/effect need counting a
/// candidate's own attachments and a min()-style computed value, neither of which the JSON
/// vocabulary supports.
/// </summary>
public class MountainsAnvilCastleTests
{
    [Test]
    public void Provisions_StartingHonorAndFateIncomeAndStrengthBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var stronghold = new Card { Id = "mountain-s-anvil-castle", Type = CardType.Stronghold, Controller = p1, PrintedHonor = 11, PrintedFateIncome = 7, PrintedStrengthBonus = 2 };
        p1.Stronghold = stronghold;

        game.SetHonorFromStronghold(p1);

        Assert.That(p1.Honor, Is.EqualTo(11));
        Assert.That(game.FateIncomeFor(p1), Is.EqualTo(7));
        Assert.That(game.StrongholdStrengthBonusFor(p1), Is.EqualTo(2));
    }

    [Test]
    public void WithoutAStrongholdSet_ThrowsRatherThanSilentlyDoingNothing()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        Assert.Throws<InvalidOperationException>(() => game.SetHonorFromStronghold(p1));
        Assert.Throws<InvalidOperationException>(() => game.FateIncomeFor(p1));
        Assert.That(game.StrongholdStrengthBonusFor(p1), Is.EqualTo(0), "strengthBonus alone defaults rather than requiring a stronghold");
    }

    private static (GameState Game, Card Stronghold, Card Target) NewGameDuringAConflict()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var stronghold = new Card { Id = "mountain-s-anvil-castle", Type = CardType.Stronghold, Controller = p1 };
        var target = new Card { Id = "attached-character", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2, PrintedPoliticalSkill = 2 };
        p1.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(target);
        game.CurrentConflict = conflict;

        return (game, stronghold, target);
    }

    [Test]
    public void BowingGivesAParticipantWithOneAttachmentPlusOnePlusOne()
    {
        var (game, stronghold, target) = NewGameDuringAConflict();
        var attachment = new Card { Id = "an-attachment", Type = CardType.Attachment, Controller = game.Player1, AttachedTo = target };
        game.Player1.PlayArea.Add(attachment);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = stronghold, Target = target };

        new MountainsAnvilCastleBonusForAttachments().Execute(context);

        Assert.That(stronghold.Bowed, Is.True);
        Assert.That(game.EffectiveMilitarySkill(target), Is.EqualTo(3));
        Assert.That(game.EffectivePoliticalSkill(target), Is.EqualTo(3));
    }

    [Test]
    public void BowingGivesAParticipantWithTwoOrMoreAttachmentsPlusTwoPlusTwo()
    {
        var (game, stronghold, target) = NewGameDuringAConflict();
        var attachment1 = new Card { Id = "attachment-1", Type = CardType.Attachment, Controller = game.Player1, AttachedTo = target };
        var attachment2 = new Card { Id = "attachment-2", Type = CardType.Attachment, Controller = game.Player1, AttachedTo = target };
        var attachment3 = new Card { Id = "attachment-3", Type = CardType.Attachment, Controller = game.Player1, AttachedTo = target };
        game.Player1.PlayArea.Add(attachment1);
        game.Player1.PlayArea.Add(attachment2);
        game.Player1.PlayArea.Add(attachment3);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = stronghold, Target = target };

        new MountainsAnvilCastleBonusForAttachments().Execute(context);

        Assert.That(game.EffectiveMilitarySkill(target), Is.EqualTo(4), "capped at +2 even with 3 attachments");
        Assert.That(game.EffectivePoliticalSkill(target), Is.EqualTo(4));
    }

    [Test]
    public void ExpiresAtEndOfConflict()
    {
        var (game, stronghold, target) = NewGameDuringAConflict();
        var attachment = new Card { Id = "an-attachment", Type = CardType.Attachment, Controller = game.Player1, AttachedTo = target };
        game.Player1.PlayArea.Add(attachment);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = stronghold, Target = target };
        new MountainsAnvilCastleBonusForAttachments().Execute(context);

        game.EndConflict();

        Assert.That(game.EffectiveMilitarySkill(target), Is.EqualTo(2));
    }

    [Test]
    public void ATargetWithNoAttachments_Throws()
    {
        var (game, stronghold, target) = NewGameDuringAConflict();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = stronghold, Target = target };

        Assert.Throws<InvalidOperationException>(() => new MountainsAnvilCastleBonusForAttachments().Execute(context));
    }
}
