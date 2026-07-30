using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki GameActions.ts dishonor: dishonor a character (mutually exclusive with
/// honored). Checked in Execute (throws), not CanAffect - see MoveToConflictGameActionHandler's
/// doc comment for why: a restriction like steward-of-law's "cannot receive dishonor
/// tokens" must block every call path, not just the shared-target-race one CanAffect covers.
///
/// Checks for a young-rumormonger replacement effect first (see its own doc comment for why
/// this lives here instead of a caller-invoked script like every other event-shaped card) -
/// if one applies, this recurses into itself against the redirected target instead of
/// dishonoring context.Target. Runs before the receiveDishonorToken restriction check, so a
/// restricted original target can still be legally redirected away rather than throwing.
/// </summary>
public sealed class DishonorGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("dishonor requires context.Target to be set.");

        if (!context.RedirectApplied && YoungRumormongerRedirectHonorOrDishonor.TryFindRedirect(context.Game, context.Target, out var rumormonger, out var newTarget))
        {
            var redirectContext = new AbilityContext
            {
                Game = context.Game, Player = rumormonger.Controller, Source = rumormonger,
                Target = context.Target, SecondTarget = newTarget, ChosenChoice = "Dishonor"
            };
            new YoungRumormongerRedirectHonorOrDishonor().Execute(redirectContext);
            return;
        }

        if (context.Game.IsRestrictedFrom(context.Target, "receiveDishonorToken"))
            throw new InvalidOperationException($"'{context.Target.Id}' cannot receive a dishonor token.");

        context.Target.IsDishonored = true;
        context.Target.IsHonored = false;
    }
}
