using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class VengefulBerserkerFirerTests
{
    private static (Player p1, GameState game, Card berserker, Card ally) NewScenario(int berserkerSkill = 3)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        game.CurrentConflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };

        var berserker = new Card { Id = "vengeful-berserker", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = berserkerSkill };
        var ally = new Card { Id = "ally", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(berserker);
        p1.PlayArea.Add(ally);

        return (p1, game, berserker, ally);
    }

    [Test]
    public void FireEligibleReactions_WhenAnAllyLeavesPlayDuringAConflict_DoublesMilitarySkill()
    {
        var (p1, game, berserker, ally) = NewScenario(berserkerSkill: 3);
        p1.PlayArea.Remove(ally);
        p1.Discard.Add(ally);

        VengefulBerserkerFirer.FireEligibleReactions(game, ally);

        Assert.That(game.EffectiveMilitarySkill(berserker), Is.EqualTo(6));
    }

    [Test]
    public void FireEligibleReactions_OutsideAConflict_DoesNotFire()
    {
        var (p1, game, berserker, ally) = NewScenario();
        game.CurrentConflict = null;
        p1.PlayArea.Remove(ally);
        p1.Discard.Add(ally);

        VengefulBerserkerFirer.FireEligibleReactions(game, ally);

        Assert.That(game.EffectiveMilitarySkill(berserker), Is.EqualTo(3));
    }

    [Test]
    public void DiscardFromPlayGameActionHandler_DiscardingAnAlly_FiresTheReaction()
    {
        var (p1, game, berserker, ally) = NewScenario(berserkerSkill: 3);

        var context = new AbilityContext { Game = game, Player = p1, Source = ally, Target = ally };
        new DiscardFromPlayGameActionHandler().Execute(context, null);

        Assert.That(p1.Discard, Contains.Item(ally));
        Assert.That(game.EffectiveMilitarySkill(berserker), Is.EqualTo(6));
    }
}
