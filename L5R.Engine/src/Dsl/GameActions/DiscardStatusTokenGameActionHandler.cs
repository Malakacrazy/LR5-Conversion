using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki GameActions.ts discardStatusToken (DiscardStatusAction): clears a character's
/// status-token slot. Ringteki models honored/dishonored as a real StatusToken object held
/// in one slot ("personalHonor"); this engine flattens that onto Card.IsHonored/IsDishonored
/// directly, so "discard the token" is simply clearing both. Recipient defaults to
/// context.Target, but params can carry its own "target" override (soshi-illusionist's
/// "target.personalHonor" contextPath) - same optional-override convention as
/// CardLastingEffectGameActionHandler.
/// </summary>
public sealed class DiscardStatusTokenGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var target = parameters?.TryGetProperty("target", out var targetElement) == true
            ? TargetResolver.ResolveAllCardsMatching(targetElement, context).Single()
            : context.Target ?? throw new InvalidOperationException("discardStatusToken requires context.Target to be set.");

        target.IsHonored = false;
        target.IsDishonored = false;
    }
}
