using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// shiba-tsukune: as the conflict phase ends, resolve up to 2 unclaimed rings' own effects.
/// Fires as a one-shot hook at the very end of GameLoop.ConflictPhaseStep, right before
/// GameState.AdvancePhase() flips CurrentPhase to Fate - the script's own gate requires
/// CurrentPhase to still read Conflict, matching "as the phase ends" rather than "once it has
/// ended". Same trivial-bot default as AkodoToturiBotAction's own ring resolution: TargetRing
/// is set but ChosenChoice/Target are left null, which every element's own resolve handler
/// already treats as a legal "don't resolve" no-op (ResolveConflictRingGameActionHandler's
/// own doc comment) - this adopts the action as legally reachable without inventing a new
/// per-element choice policy for a single card.
/// </summary>
public static class ShibaTsukuneFirer
{
    private const int MaxRingsPerFiring = 2;

    public static void FireIfLegal(GameState game, Player player)
    {
        if (game.CurrentPhase != Phase.Conflict)
            return;

        foreach (var tsukune in player.PlayArea.Where(c => c.Id == "shiba-tsukune" && !game.IsBlanked(c)).ToList())
        {
            foreach (var ring in game.Rings.Where(r => r.IsUnclaimed).Take(MaxRingsPerFiring).ToList())
            {
                var context = new AbilityContext { Game = game, Player = player, Source = tsukune, TargetRing = ring };
                new ShibaTsukuneResolveUpToTwoRings().Execute(context);
            }
        }
    }
}
