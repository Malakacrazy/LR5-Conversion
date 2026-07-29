using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki MoveToConflictAction: adds context.Target to the current conflict, as an
/// attacker if its controller is the attacking player, otherwise as a defender. Real
/// canAffect also checks the card isn't already participating, canParticipateAsAttacker/
/// Defender restrictions, and play-area location - not enforced here, matching the same
/// trust-the-caller convention already used for chosenTarget elsewhere in this interpreter.
/// </summary>
public sealed class MoveToConflictGameActionHandler : IGameActionHandler
{
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
