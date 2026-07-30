using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// watch-commander: a reaction to the opponent playing any card at all - unlike every card
/// this session initially rejected as a "near-zero precondition" scriptOverride reaction
/// (it really does have essentially no gate beyond "opponent played *a* card"), this one
/// turns out to be perfectly safe once invoked as a one-shot hook at the exact moment a card
/// is played (PlayCardGameActionHandler.Execute, already established for
/// TriggeredReactionFirer's onCharacterEntersPlay) rather than through the general
/// ChooseScriptedAction poll - it was never the trigger condition itself that was unsound,
/// only treating it as a re-pollable state fact would have been (it would fire every action
/// window turn forever, since nothing about "an opponent played a card at some point" ever
/// resets on its own).
///
/// Scans the opponent's own play area (not a general board-wide search) since watch-commander
/// is non-unique - a player could control more than one copy, each independently attached to
/// a different participating character, and each should react independently.
/// </summary>
public static class WatchCommanderFirer
{
    public static void FireEligibleReactions(GameState game, Player playingPlayer)
    {
        var opponent = game.Opponent(playingPlayer);

        foreach (var watchCommander in opponent.PlayArea.Where(c => c.Id == "watch-commander").ToList())
        {
            if (game.IsBlanked(watchCommander))
                continue;

            if (watchCommander.AttachedTo is not { } parent || !IsParticipating(game, parent))
                continue;

            var context = new AbilityContext { Game = game, Player = opponent, Source = watchCommander };
            new WatchCommanderLoseHonorOnOpponentCardPlayed().Execute(context);
        }
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
