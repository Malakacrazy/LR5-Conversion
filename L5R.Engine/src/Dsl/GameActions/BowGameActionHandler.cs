using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki GameActions.ts bow: bow a card. Can't affect an already-bowed card. Checked in
/// Execute (throws), not CanAffect - see MoveToConflictGameActionHandler's doc comment for
/// why: a restriction like sashimono's "doesNotBow" (while attached, during a military
/// conflict) must block every call path, not just the shared-target-race one CanAffect covers.
/// </summary>
public sealed class BowGameActionHandler : IGameActionHandler
{
    public bool CanAffect(AbilityContext context, JsonElement? parameters) =>
        context.Target is { Bowed: false };

    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("bow requires context.Target to be set.");

        if (context.Game.IsRestrictedFrom(context.Target, "bow"))
            throw new InvalidOperationException($"'{context.Target.Id}' cannot be bowed.");

        context.Target.Bowed = true;
    }
}
