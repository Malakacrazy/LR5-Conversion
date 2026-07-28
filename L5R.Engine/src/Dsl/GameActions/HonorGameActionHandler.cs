using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>ringteki GameActions.ts honor: honor a character (mutually exclusive with dishonored).</summary>
public sealed class HonorGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("honor requires context.Target to be set.");

        context.Target.IsHonored = true;
        context.Target.IsDishonored = false;
    }
}
