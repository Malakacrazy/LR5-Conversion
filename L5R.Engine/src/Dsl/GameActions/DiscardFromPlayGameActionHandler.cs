using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>ringteki GameActions.ts discardFromPlay: discard a card from play.</summary>
public sealed class DiscardFromPlayGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("discardFromPlay requires context.Target to be set.");

        ZoneMover.MoveTo(context.Target, context.Target.Controller.Discard, "discard");
    }
}
