using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki GameActions.ts honor: honor a character (mutually exclusive with dishonored).
/// Checks for a young-rumormonger replacement effect first (see its own doc comment for why
/// this lives here instead of a caller-invoked script like every other event-shaped card) -
/// if one applies, this recurses into itself against the redirected target instead of
/// honoring context.Target.
/// </summary>
public sealed class HonorGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("honor requires context.Target to be set.");

        if (!context.RedirectApplied && YoungRumormongerRedirectHonorOrDishonor.TryFindRedirect(context.Game, context.Target, out var rumormonger, out var newTarget))
        {
            var redirectContext = new AbilityContext
            {
                Game = context.Game, Player = rumormonger.Controller, Source = rumormonger,
                Target = context.Target, SecondTarget = newTarget, ChosenChoice = "Honor"
            };
            new YoungRumormongerRedirectHonorOrDishonor().Execute(redirectContext);
            return;
        }

        context.Target.IsHonored = true;
        context.Target.IsDishonored = false;
    }
}
