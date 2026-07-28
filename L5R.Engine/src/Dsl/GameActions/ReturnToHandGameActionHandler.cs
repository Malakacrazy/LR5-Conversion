using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>ringteki GameActions.ts returnToHand: move a card to its controller's hand.</summary>
public sealed class ReturnToHandGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("returnToHand requires context.Target to be set.");

        ZoneMover.MoveTo(context.Target, context.Target.Controller.Hand, "hand");
    }
}
