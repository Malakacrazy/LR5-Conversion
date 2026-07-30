using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// way-of-the-unicorn: an event played from hand during the Fate phase, before the
/// automatic first-player-token pass, to keep the token for another round. FatePhaseStep has
/// no generic hand-play window (unlike Dynasty's RunPlayWindow/the pre-conflict and
/// mid-conflict windows) - real rules don't offer a general action window here either, only
/// this one specific reactive play, so a small targeted offerer checked right before
/// GameState.AdvancePhase() fits better than adding a whole new generic window. "Always play
/// when legal" - keeping the token is unconditionally good for whoever currently holds it,
/// same trivial-bot heuristic as every other adopted card.
/// </summary>
public static class WayOfTheUnicornOfferer
{
    public static void TryPlay(GameState game, Player activePlayer)
    {
        var card = activePlayer.Hand.FirstOrDefault(c => c.Id == "way-of-the-unicorn");
        if (card is null)
            return;

        var cost = game.EffectiveCost(card, activePlayer);
        if (activePlayer.Fate < cost)
            return;

        activePlayer.Fate -= cost;

        var context = new AbilityContext { Game = game, Player = activePlayer, Source = card };
        new WayOfTheUnicornKeepFirstPlayerToken().Execute(context);

        ZoneMover.MoveTo(card, activePlayer.Discard, "discard");
    }
}
