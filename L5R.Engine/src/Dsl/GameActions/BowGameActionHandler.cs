using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki GameActions.ts bow: bow a card. Can't affect an already-bowed card. Checked in
/// Execute (throws), not CanAffect - see MoveToConflictGameActionHandler's doc comment for
/// why: a restriction like sashimono's "doesNotBow" (while attached, during a military
/// conflict) must block every call path, not just the shared-target-race one CanAffect covers.
/// Also offers ready-for-battle a chance to react if the target's own controller didn't
/// cause the bow themselves - see ReadyForBattleFirer's own doc comment.
/// </summary>
public sealed class BowGameActionHandler : IGameActionHandler
{
    public bool CanAffect(AbilityContext context, JsonElement? parameters) =>
        context.Target is { Bowed: false };

    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("bow requires context.Target to be set.");

        if (context.Game.IsRestrictedFrom(context.Target, "bow", context.Source))
            throw new InvalidOperationException($"'{context.Target.Id}' cannot be bowed.");

        context.Target.Bowed = true;

        ReadyForBattleFirer.FireIfLegal(context.Game, context.Player, context.Target);
    }
}
