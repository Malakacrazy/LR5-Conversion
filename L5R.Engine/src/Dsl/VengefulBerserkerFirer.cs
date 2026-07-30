using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// vengeful-berserker: after another character its controller controls leaves play during a
/// conflict, double this character's military skill until the end of the conflict. Its own
/// script trusts a caller-supplied context.Target (the departed character) - no event bus
/// records "a character just left play" automatically, so this fires it directly at the one
/// place a character is discarded from play as a JSON-driven gameAction
/// (DiscardFromPlayGameActionHandler.Execute, also the shape GameActionRegistry registers as
/// "discardFromPlay"). Doesn't cover a sacrifice cost's own departure - SacrificeCostHandler
/// calls ZoneMover.MoveTo directly rather than through this handler, and ZoneMover itself has
/// no GameState reference to check from (touching its signature would ripple through every
/// one of its dozens of call sites) - same "hook the canonical shared primitive, not every
/// call site" scope as ikoma-prodigy/young-rumormonger.
/// </summary>
public static class VengefulBerserkerFirer
{
    public static void FireEligibleReactions(GameState game, Card departedCharacter)
    {
        if (departedCharacter.Type != CardType.Character || game.CurrentConflict is null)
            return;

        var controller = departedCharacter.Controller;
        foreach (var berserker in controller.PlayArea.Where(c => c.Id == "vengeful-berserker" && c != departedCharacter && !game.IsBlanked(c)).ToList())
        {
            var context = new AbilityContext { Game = game, Player = controller, Source = berserker, Target = departedCharacter };
            new VengefulBerserkerDoubleMilitaryOnAllyLeavingPlay().Execute(context);
        }
    }
}
