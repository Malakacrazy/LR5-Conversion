using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// intimidating-hida: after the opponent passes on declaring a conflict as the attacking
/// player, that player loses 1 honor. "Passing" isn't otherwise modeled as an event in this
/// engine, so this fires directly at GameLoop.ConflictPhaseStep's own pass branch, the one
/// place a player's turn to declare ends without a declaration - constructing a throwaway
/// Conflict carrying just the asserted fact (AttackingPlayer = the player who just passed),
/// matching the script's own doc comment. No real conflict is in progress at this exact
/// moment (game.CurrentConflict is always null here - a real declaration would have gone
/// through ConflictResolver.Resolve instead, which clears it again via EndConflict), so it's
/// safe to set and immediately clear game.CurrentConflict around the call.
/// </summary>
public static class IntimidatingHidaFirer
{
    public static void FireIfLegal(GameState game, Player passingPlayer)
    {
        var beneficiary = game.Opponent(passingPlayer);

        var hidas = beneficiary.PlayArea.Where(c => c.Id == "intimidating-hida" && !game.IsBlanked(c)).ToList();
        if (hidas.Count == 0)
            return;

        game.CurrentConflict = new Conflict { AttackingPlayer = passingPlayer, DefendingPlayer = beneficiary };
        try
        {
            foreach (var hida in hidas)
            {
                var context = new AbilityContext { Game = game, Player = beneficiary, Source = hida };
                new IntimidatingHidaLoseHonorOnOpponentPass().Execute(context);
            }
        }
        finally
        {
            game.CurrentConflict = null;
        }
    }
}
