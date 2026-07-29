using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki MoveToConflictAction: adds context.Target to the current conflict, as an
/// attacker if its controller is the attacking player, otherwise as a defender. canAffect
/// requires an active conflict the card isn't already part of - load-bearing for
/// favorable-ground's [sendHome, moveToConflict] pair, exactly like bow/ready: only one of
/// the two can ever apply to a given character at once. Real canAffect also checks
/// canParticipateAsAttacker/Defender restrictions and play-area location - not enforced
/// here, matching the same trust-the-caller convention used for chosenTarget elsewhere.
/// </summary>
public sealed class MoveToConflictGameActionHandler : IGameActionHandler
{
    public bool CanAffect(AbilityContext context, JsonElement? parameters) =>
        context.Game.CurrentConflict is { } conflict && context.Target is { } target
        && !conflict.Attackers.Contains(target) && !conflict.Defenders.Contains(target);

    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("moveToConflict requires context.Target to be set.");

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException("moveToConflict requires an active conflict.");

        if (context.Target.Controller == conflict.AttackingPlayer)
            conflict.Attackers.Add(context.Target);
        else
            conflict.Defenders.Add(context.Target);
    }
}
