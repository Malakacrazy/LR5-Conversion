using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// steadfast-samurai: after the fate phase begins, if its controller's honor is at least 5
/// more than the opponent's, this character can't be discarded or have fate removed until
/// the end of the phase. Fires as a one-shot hook at the very top of GameLoop.FatePhaseStep,
/// before the fate-decrement/no-fate-discard loop even runs - the restriction has to already
/// be in place before that loop asks GameState.IsRestrictedFrom, which it now does (see
/// GameLoop's own comment on that loop for why it previously bypassed both handlers
/// entirely, going straight at Card.Fate/PlayArea rather than through
/// RemoveFateGameActionHandler/DiscardFromPlayGameActionHandler).
/// </summary>
public static class SteadfastSamuraiFirer
{
    public static void FireIfLegal(GameState game, Player player)
    {
        var opponent = game.Opponent(player);
        if (player.Honor < opponent.Honor + 5)
            return;

        foreach (var samurai in player.PlayArea.Where(c => c.Id == "steadfast-samurai" && !game.IsBlanked(c)).ToList())
        {
            var context = new AbilityContext { Game = game, Player = player, Source = samurai };
            new SteadfastSamuraiHonorThresholdProtection().Execute(context);
        }
    }
}
