using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// young-rumormonger: when a character would be honored or dishonored, choose a different
/// character controlled by the same player to receive it instead - a replacement effect
/// (it changes which character the honor/dishonor lands on), not a reaction that runs after
/// the fact. Unlike every other event-shaped script this session (which needs a caller to
/// notice the event happened and invoke it by hand), a replacement effect has to run at the
/// exact moment of application to actually replace anything - so HonorGameActionHandler/
/// DishonorGameActionHandler call TryFindRedirect themselves at the top of Execute, before
/// applying anything, and recurse into this script when a redirect is available. This is the
/// one card in the whole scriptOverride backlog that's wired into shared production handlers
/// rather than discovered/invoked via IBotScriptAction - it isn't a discrete action a bot
/// "chooses" to take, it's an always-on effect exactly like the real card's text box.
///
/// context.Target carries the original (redirected-away-from) target; context.SecondTarget
/// (shameful-display's own two-target field) carries the new target that actually receives
/// the honor/dishonor; context.ChosenChoice ("Honor"/"Dishonor", same convention as
/// court-games/asako-diplomat) selects which one, matching whichever event actually fired.
/// </summary>
public sealed class YoungRumormongerRedirectHonorOrDishonor : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var rumormonger = context.Source;

        var originalTarget = context.Target
            ?? throw new InvalidOperationException($"'{rumormonger.Id}' requires context.Target (the original target) to be set.");

        var newTarget = context.SecondTarget
            ?? throw new InvalidOperationException($"'{rumormonger.Id}' requires context.SecondTarget (the new target) to be set.");

        if (newTarget == originalTarget)
            throw new InvalidOperationException($"'{rumormonger.Id}' requires a different character than '{originalTarget.Id}'.");

        if (newTarget.Controller != originalTarget.Controller)
            throw new InvalidOperationException($"'{newTarget.Id}' must be controlled by the same player as '{originalTarget.Id}'.");

        context.Target = newTarget;

        // Set regardless of how this script was reached (recursed into from Honor/
        // DishonorGameActionHandler's own guard, or invoked directly) - without this, a
        // direct caller's context would still read RedirectApplied == false, and the
        // handler call below would immediately re-trigger and ping-pong between the two
        // targets via TryFindRedirect finding this same young-rumormonger again.
        context.RedirectApplied = true;

        switch (context.ChosenChoice)
        {
            case "Honor":
                new HonorGameActionHandler().Execute(context, null);
                break;
            case "Dishonor":
                new DishonorGameActionHandler().Execute(context, null);
                break;
            default:
                throw new InvalidOperationException($"'{rumormonger.Id}' requires context.ChosenChoice to be 'Honor' or 'Dishonor'.");
        }
    }

    /// <summary>
    /// Finds an unblanked young-rumormonger controlled by either player, plus a legal
    /// redirect target (a different character controlled by originalTarget's own
    /// controller). "First candidate" heuristic, same convention as every other bot adapter
    /// this session - not the actual card's own optional "may" choice, matching the trivial
    /// bot's general "always take an available action" behavior applied automatically here.
    /// If both players somehow control one, the first found (Player1's) wins - an accepted
    /// simplification, not a modeled priority rule.
    /// </summary>
    internal static bool TryFindRedirect(GameState game, Card originalTarget, out Card rumormonger, out Card newTarget)
    {
        var candidate = game.Player1.PlayArea.Concat(game.Player2.PlayArea)
            .FirstOrDefault(c => c.Id == "young-rumormonger" && !game.IsBlanked(c));

        var redirectTarget = candidate is not null
            ? originalTarget.Controller.PlayArea.FirstOrDefault(c => c.Type == CardType.Character && c != originalTarget)
            : null;

        if (candidate is null || redirectTarget is null)
        {
            rumormonger = null!;
            newTarget = null!;
            return false;
        }

        rumormonger = candidate;
        newTarget = redirectTarget;
        return true;
    }
}
