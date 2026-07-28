using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>ringteki GameActions.ts ready: ready a card. Can't affect an already-ready card.</summary>
public sealed class ReadyGameActionHandler : IGameActionHandler
{
    public bool CanAffect(AbilityContext context, JsonElement? parameters) =>
        context.Target is { Bowed: true };

    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("ready requires context.Target to be set.");

        context.Target.Bowed = false;
    }
}
