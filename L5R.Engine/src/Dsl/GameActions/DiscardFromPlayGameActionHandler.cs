using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki GameActions.ts discardFromPlay: discard a card from play. Checks
/// IsRestrictedFrom "discardFromPlay" (steadfast-samurai's own cardCannot) - previously
/// unchecked here since no ported card needed to block this action before now.
/// </summary>
public sealed class DiscardFromPlayGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("discardFromPlay requires context.Target to be set.");

        if (context.Game.IsRestrictedFrom(context.Target, "discardFromPlay", context.Source))
            throw new InvalidOperationException($"'{context.Target.Id}' cannot be discarded.");

        ZoneMover.MoveTo(context.Target, context.Target.Controller.Discard, "discard");
    }
}
