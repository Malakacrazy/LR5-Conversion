using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// lion-s-pride-brawler: while attacking, bow a character with (effective) military skill
/// at most this character's own. Candidate-vs-source dynamic stat comparison, beyond
/// compareStat's candidate-vs-literal shape - a plain GameState.EffectiveMilitarySkill
/// comparison here instead.
/// </summary>
public sealed class LionsPrideBrawlerBowLowerMilitarySkill : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var brawler = context.Source;

        if (context.Game.CurrentConflict?.Attackers.Contains(brawler) != true)
            throw new InvalidOperationException($"'{brawler.Id}' can only be used while attacking.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{brawler.Id}' requires context.Target to be set.");

        if (context.Game.EffectiveMilitarySkill(target) > context.Game.EffectiveMilitarySkill(brawler))
            throw new InvalidOperationException($"'{target.Id}''s military skill exceeds '{brawler.Id}''s.");

        new BowGameActionHandler().Execute(context, null);
    }
}
