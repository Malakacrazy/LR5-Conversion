using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>ringteki SendHomeAction: removes context.Target from the current conflict's attackers/defenders.</summary>
public sealed class SendHomeGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("sendHome requires context.Target to be set.");

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException("sendHome requires an active conflict.");

        conflict.Attackers.Remove(context.Target);
        conflict.Defenders.Remove(context.Target);
    }
}
