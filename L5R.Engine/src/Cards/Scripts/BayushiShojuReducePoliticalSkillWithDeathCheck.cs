using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// bayushi-shoju: while participating in a political conflict, reduce an opponent's
/// participating character's political skill by 1 until the end of the conflict,
/// discarding it if that drops its effective political skill below 1. Needs a nested
/// delayedEffect as one of the lasting effect's array entries - inlined here as an
/// immediate check right after applying the modifier (this engine resolves each script
/// call as a single one-shot step, not a standing listener re-checked on every later
/// change, matching every other "delayedEffect" gap this session). "limit: perRound(2)"
/// needs no work, matching every other "max"/"limit" field's established no-op precedent.
/// </summary>
public sealed class BayushiShojuReducePoliticalSkillWithDeathCheck : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var shoju = context.Source;

        if (!IsParticipating(context.Game, shoju))
            throw new InvalidOperationException($"'{shoju.Id}' can only be used while participating.");

        var conflict = context.Game.CurrentConflict!;
        if (conflict.ConflictType != "political")
            throw new InvalidOperationException($"'{shoju.Id}' can only be used during a political conflict.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{shoju.Id}' requires context.Target to be set.");

        if (target.Controller != context.Game.Opponent(context.Player))
            throw new InvalidOperationException($"'{target.Id}' must be controlled by the opponent.");

        if (!IsParticipating(context.Game, target))
            throw new InvalidOperationException($"'{target.Id}' is not participating.");

        context.Game.LastingEffects.Add(new LastingEffect { Target = target, Stat = "political", Value = -1, Duration = "untilEndOfConflict" });

        if (context.Game.EffectivePoliticalSkill(target) < 1)
            new DiscardFromPlayGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
