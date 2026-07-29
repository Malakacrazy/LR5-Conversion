using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki GameActions.ts dishonor: dishonor a character (mutually exclusive with
/// honored). Checked in Execute (throws), not CanAffect - see MoveToConflictGameActionHandler's
/// doc comment for why: a restriction like steward-of-law's "cannot receive dishonor
/// tokens" must block every call path, not just the shared-target-race one CanAffect covers.
/// </summary>
public sealed class DishonorGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("dishonor requires context.Target to be set.");

        if (context.Game.IsRestrictedFrom(context.Target, "receiveDishonorToken"))
            throw new InvalidOperationException($"'{context.Target.Id}' cannot receive a dishonor token.");

        context.Target.IsDishonored = true;
        context.Target.IsHonored = false;
    }
}
