using System.Linq;
using L5R.Engine.State;

namespace L5R.Engine.Abilities;

/// <summary>
/// The public surface the roadmap calls for: given engine state and the acting player,
/// return every action that could legally be triggered right now. This is what a future
/// command from the client gets validated against, replacing the implicit validation
/// scattered through ringteki's React client.
/// </summary>
public static class LegalActions
{
    public static IReadOnlyList<CardAction> GetLegalActions(GameState game, Player actingPlayer)
    {
        var result = new List<CardAction>();

        foreach (var card in game.AllCards())
        {
            foreach (var action in card.Actions)
            {
                var context = new AbilityContext { Game = game, Player = actingPlayer, Source = card };
                if (action.MeetsRequirements(context))
                    result.Add(action);
            }
        }

        return result;
    }

    /// <summary>
    /// A different concern from GetLegalActions: "which card can I play from hand/a revealed
    /// province" rather than "which explicit abilities.actions[] entry can I activate" - every
    /// card is implicitly "playable" by virtue of PlayCardGameActionHandler's own preconditions
    /// (affordable, not play-restricted, PlayScript.CanPlay), not something CardFactory bridges
    /// into Card.Actions. Attachments are excluded - they need a PlayAttachTarget the caller
    /// must choose, out of scope for GameLoop's v1 trivial bots (see GameLoop's own doc comment).
    /// </summary>
    public static IReadOnlyList<Card> GetLegalPlays(GameState game, Player player, string location)
    {
        IEnumerable<Card> candidates = location switch
        {
            "hand" => player.Hand,
            "province" => player.Provinces.Where(card => !card.Facedown),
            _ => throw new NotSupportedException($"GetLegalPlays does not support location '{location}'.")
        };

        var result = new List<Card>();
        foreach (var card in candidates)
        {
            if (card.Type is CardType.Attachment or CardType.Province or CardType.Stronghold or CardType.Role)
                continue;

            if (game.IsPlayerRestrictedFrom(player, "play", card))
                continue;

            var context = new AbilityContext { Game = game, Player = player, Source = card };
            if (card.PlayScript?.CanPlay(context) == false)
                continue;

            if (game.EffectiveCost(card, player) > player.Fate)
                continue;

            result.Add(card);
        }

        return result;
    }
}
