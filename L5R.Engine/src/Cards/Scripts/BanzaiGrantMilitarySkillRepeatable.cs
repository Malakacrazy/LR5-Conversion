using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// banzai: during a conflict, give a participating character +2 military skill until the
/// end of the conflict. Ringteki's own "may lose 1 honor to resolve this ability again"
/// chain (itself capped - a second re-resolution only offers "lose 1 honor for no effect",
/// never a third real application) isn't a true recursion loop, so it needs no bespoke
/// stateful-interaction mechanism: one Execute call applies one +2 grant, and "resolve
/// again" is simply the caller paying the honor cost (LoseHonorGameActionHandler) and
/// calling Execute a second time with a new target - the same "up to N times is the
/// caller's responsibility" convention already used for shiba-tsukune/giver-of-gifts.
/// </summary>
public sealed class BanzaiGrantMilitarySkillRepeatable : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var banzai = context.Source;

        if (context.Game.CurrentConflict is null)
            throw new InvalidOperationException($"'{banzai.Id}' requires an active conflict.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{banzai.Id}' requires context.Target to be set.");

        if (!IsParticipating(context.Game, target))
            throw new InvalidOperationException($"'{target.Id}' is not participating.");

        context.Game.LastingEffects.Add(new LastingEffect { Target = target, Stat = "military", Value = 2, Duration = "untilEndOfConflict" });
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
