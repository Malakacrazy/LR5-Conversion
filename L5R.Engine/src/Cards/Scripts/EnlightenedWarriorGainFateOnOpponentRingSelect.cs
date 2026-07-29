using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// enlightened-warrior: after the opponent declares a conflict on a ring that had fate on
/// it, place 1 fate on this character ("event.ringFate > 0 && event.conflict.attackingPlayer
/// === context.player.opponent"). The declared ring is context.TargetRing (caller-supplied,
/// same convention as every other ring-scoped field in this engine) rather than a separate
/// event.ringFate value. placeFate's own default target (context.Target, defaulting to
/// context.Source per the shared no-target-block convention) already means "this character".
/// </summary>
public sealed class EnlightenedWarriorGainFateOnOpponentRingSelect : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var warrior = context.Source;
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{warrior.Id}' requires an active conflict.");

        if (conflict.AttackingPlayer != context.Game.Opponent(context.Player))
            throw new InvalidOperationException($"'{warrior.Id}' can only trigger when the opponent is the attacking player.");

        var ring = context.TargetRing
            ?? throw new InvalidOperationException($"'{warrior.Id}' requires context.TargetRing to be set.");

        if (ring.Fate <= 0)
            throw new InvalidOperationException($"'{ring.Element}' ring had no fate on it.");

        context.Target = warrior;
        new PlaceFateGameActionHandler().Execute(context, null);
    }
}
