using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class BanzaiTests
{
    [Test]
    public void GivesAParticipatingCharacterPlusTwoMilitarySkill()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var banzai = new Card { Id = "banzai", Type = CardType.Event, Controller = p1 };
        var target = new Card { Id = "some-character", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2 };
        p1.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(target);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = banzai, Target = target };

        new BanzaiGrantMilitarySkillRepeatable().Execute(context);

        Assert.That(game.EffectiveMilitarySkill(target), Is.EqualTo(4));
    }

    [Test]
    public void ResolvingAgainAfterPayingHonor_StacksTheBonus()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var banzai = new Card { Id = "banzai", Type = CardType.Event, Controller = p1 };
        var firstTarget = new Card { Id = "first-character", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2 };
        var secondTarget = new Card { Id = "second-character", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 1 };
        p1.PlayArea.Add(firstTarget);
        p1.PlayArea.Add(secondTarget);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(firstTarget);
        conflict.Attackers.Add(secondTarget);
        game.CurrentConflict = conflict;

        new BanzaiGrantMilitarySkillRepeatable().Execute(new AbilityContext { Game = game, Player = p1, Source = banzai, Target = firstTarget });

        // LoseHonorGameActionHandler's default target is context.Game.Opponent(context.Player)
        // (ringteki's own PlayerAction default) - banzai's "resolve again" cost is explicitly
        // self-targeted (loseHonor({target: context.player})), which that handler has no
        // override for, so the cost is paid directly here rather than via a mismatched call.
        p1.Honor -= 1;
        new BanzaiGrantMilitarySkillRepeatable().Execute(new AbilityContext { Game = game, Player = p1, Source = banzai, Target = secondTarget });

        Assert.That(p1.Honor, Is.EqualTo(2));
        Assert.That(game.EffectiveMilitarySkill(firstTarget), Is.EqualTo(4));
        Assert.That(game.EffectiveMilitarySkill(secondTarget), Is.EqualTo(3));
    }

    [Test]
    public void OnANonParticipatingCharacter_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var banzai = new Card { Id = "banzai", Type = CardType.Event, Controller = p1 };
        var nonParticipant = new Card { Id = "bench-character", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(nonParticipant);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = banzai, Target = nonParticipant };

        Assert.Throws<InvalidOperationException>(() => new BanzaiGrantMilitarySkillRepeatable().Execute(context));
    }
}
