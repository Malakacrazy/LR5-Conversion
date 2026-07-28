using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>ringteki GameActions.ts bow: bow a card. Can't affect an already-bowed card.</summary>
public sealed class BowGameActionHandler : IGameActionHandler
{
    public bool CanAffect(AbilityContext context, JsonElement? parameters) =>
        context.Target is { Bowed: false };

    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("bow requires context.Target to be set.");

        context.Target.Bowed = true;
    }
}
