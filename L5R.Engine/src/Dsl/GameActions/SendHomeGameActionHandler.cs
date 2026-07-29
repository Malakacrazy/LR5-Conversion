using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki SendHomeAction: removes context.Target from the current conflict's
/// attackers/defenders. canAffect requires the card to actually be participating -
/// load-bearing for favorable-ground's [sendHome, moveToConflict] pair, exactly like
/// bow/ready: only one of the two can ever apply to a given character at once. The
/// restriction check is in Execute (throws), not CanAffect - same reasoning as
/// BowGameActionHandler's own doc comment (borderlands-defender's "opponentsCardEffects"
/// cardCannot: sendHome must block every call path, not just the shared-target-race one).
/// </summary>
public sealed class SendHomeGameActionHandler : IGameActionHandler
{
    public bool CanAffect(AbilityContext context, JsonElement? parameters) =>
        context.Game.CurrentConflict is { } conflict && context.Target is { } target
        && (conflict.Attackers.Contains(target) || conflict.Defenders.Contains(target));

    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("sendHome requires context.Target to be set.");

        if (context.Game.IsRestrictedFrom(context.Target, "sendHome", context.Source))
            throw new InvalidOperationException($"'{context.Target.Id}' cannot be sent home.");

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException("sendHome requires an active conflict.");

        conflict.Attackers.Remove(context.Target);
        conflict.Defenders.Remove(context.Target);
    }
}
