using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// reprieve: a "wouldInterrupt" attachment - when the attached character would leave play,
/// discard this attachment instead. Same shape as StandYourGroundOfferer, offered at the
/// same shared choke point (the top of DiscardFromPlayGameActionHandler.Execute) - the card's
/// own script re-enters that same handler with context.Target reset to the attachment
/// itself, which is safe (the recursive call finds no further reprieve/stand-your-ground to
/// intercept for the attachment's own departure, so it just discards normally). Unconditionally
/// beneficial to a trivial bot whenever legal, same "always play when legal" heuristic as
/// stand-your-ground.
/// </summary>
public static class ReprieveOfferer
{
    public static bool TryInterrupt(GameState game, Card target)
    {
        var reprieve = game.AllCards()
            .FirstOrDefault(c => c.Id == "reprieve" && c.AttachedTo == target && !game.IsBlanked(c));
        if (reprieve is null)
            return false;

        var context = new AbilityContext { Game = game, Player = reprieve.Controller, Source = reprieve };
        new ReprieveDiscardInsteadOfParentLeavingPlay().Execute(context);
        return true;
    }
}
