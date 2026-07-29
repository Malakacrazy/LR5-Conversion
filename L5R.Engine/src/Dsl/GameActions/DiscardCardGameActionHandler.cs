using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki GameActions.ts discardCard: discard a card, wherever it currently is (unlike
/// discardFromPlay, this doesn't imply the card started in play - kitsuki-investigator
/// discards from the opponent's hand).
/// </summary>
public sealed class DiscardCardGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("discardCard requires context.Target to be set.");

        ZoneMover.MoveTo(context.Target, context.Target.Controller.Discard, "discard");
    }
}
