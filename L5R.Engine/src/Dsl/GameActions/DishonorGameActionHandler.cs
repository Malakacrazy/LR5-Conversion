using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>ringteki GameActions.ts dishonor: dishonor a character (mutually exclusive with honored).</summary>
public sealed class DishonorGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("dishonor requires context.Target to be set.");

        context.Target.IsDishonored = true;
        context.Target.IsHonored = false;
    }
}
