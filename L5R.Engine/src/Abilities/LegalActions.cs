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
}
