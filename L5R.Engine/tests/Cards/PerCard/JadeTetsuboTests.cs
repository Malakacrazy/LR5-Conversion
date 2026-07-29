using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// jade-tetsubo is scriptOverride'd (JadeTetsuboReturnFateFromLowerMilitaryParticipant) -
/// the first Scripts class with a real implementation, proving ICardScript.Execute end to
/// end. Its own persistentEffects block (attachmentMyControlOnly) is generic DSL territory
/// covered by GameState.IsAttachRestricted's own tests elsewhere - this file only exercises
/// the scripted action.
/// </summary>
public class JadeTetsuboTests
{
    private static (GameState Game, Card JadeTetsubo, Card Parent) NewGameWithAttachedParent(int parentMilitarySkill)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var parent = new Card { Id = "parent-character", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = parentMilitarySkill };
        var jadeTetsubo = new Card { Id = "jade-tetsubo", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(jadeTetsubo);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(parent);
        game.CurrentConflict = conflict;

        return (game, jadeTetsubo, parent);
    }

    [Test]
    public void BowingReturnsAllFateFromALowerMilitaryParticipantToItsController()
    {
        var (game, jadeTetsubo, parent) = NewGameWithAttachedParent(parentMilitarySkill: 4);
        var weakling = new Card { Id = "weakling", Type = CardType.Character, Controller = game.Player2, PrintedMilitarySkill = 1, Fate = 3 };
        game.Player2.PlayArea.Add(weakling);
        game.CurrentConflict!.Defenders.Add(weakling);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = jadeTetsubo, Target = weakling };

        new JadeTetsuboReturnFateFromLowerMilitaryParticipant().Execute(context);

        Assert.That(jadeTetsubo.Bowed, Is.True);
        Assert.That(weakling.Fate, Is.EqualTo(0));
        Assert.That(game.Player2.Fate, Is.EqualTo(3), "fate returns to the weakling's own controller, not the ability's controller");
    }

    [Test]
    public void ATargetWithEqualOrHigherMilitarySkill_Throws()
    {
        var (game, jadeTetsubo, parent) = NewGameWithAttachedParent(parentMilitarySkill: 3);
        var equal = new Card { Id = "equal-strength", Type = CardType.Character, Controller = game.Player2, PrintedMilitarySkill = 3, Fate = 2 };
        game.Player2.PlayArea.Add(equal);
        game.CurrentConflict!.Defenders.Add(equal);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = jadeTetsubo, Target = equal };

        Assert.Throws<InvalidOperationException>(() => new JadeTetsuboReturnFateFromLowerMilitaryParticipant().Execute(context));
        Assert.That(equal.Fate, Is.EqualTo(2), "nothing happened");
    }

    [Test]
    public void AlreadyBowed_Throws()
    {
        var (game, jadeTetsubo, parent) = NewGameWithAttachedParent(parentMilitarySkill: 4);
        jadeTetsubo.Bowed = true;
        var weakling = new Card { Id = "weakling", Type = CardType.Character, Controller = game.Player2, PrintedMilitarySkill = 1, Fate = 1 };
        game.Player2.PlayArea.Add(weakling);
        game.CurrentConflict!.Defenders.Add(weakling);

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = jadeTetsubo, Target = weakling };

        Assert.Throws<InvalidOperationException>(() => new JadeTetsuboReturnFateFromLowerMilitaryParticipant().Execute(context));
    }

    [Test]
    public void WhenTheAttachedCharacterIsNotParticipating_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var parent = new Card { Id = "parent-character", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 4 };
        var jadeTetsubo = new Card { Id = "jade-tetsubo", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(jadeTetsubo);

        var context = new AbilityContext { Game = game, Player = p1, Source = jadeTetsubo };

        Assert.Throws<InvalidOperationException>(() => new JadeTetsuboReturnFateFromLowerMilitaryParticipant().Execute(context));
    }
}
