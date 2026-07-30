using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// seeker-initiate: unlike keeper-initiate (its own script explicitly requires being in
/// Provinces/Discard), this script never checks its own location at all - only
/// ring.ClaimedBy and the controller's Role. It's a plain character card, reachable by the
/// normal ChooseScriptedAction scan once it's in play (the adapter adds a Location ==
/// "play area" gate the script itself doesn't check, matching kakita-asami's own precedent).
/// Same Conflict.RingClaimedThisConflict gate as akodo-toturi/doji-hotaru - ring.ClaimedBy
/// alone persists across conflicts once set, so it can't distinguish "just claimed" from
/// "claimed several conflicts ago."
/// </summary>
public sealed class SeekerInitiateBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        source.Location == "play area" && FindMatchingRing(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var ring = FindMatchingRing(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot ring.");

        var context = new AbilityContext
        {
            Game = game, Player = actingPlayer, Source = source, TargetRing = ring,
            ChosenDeckSearchCard = actingPlayer.Deck.FirstOrDefault()
        };
        new SeekerInitiateSearchTopFiveOnMatchingRingClaim().Execute(context);
    }

    private static Ring? FindMatchingRing(GameState game, Player actingPlayer)
    {
        if (actingPlayer.Role is not { } role)
            return null;

        if (game.CurrentConflict is not { RingClaimedThisConflict: true } conflict || conflict.Elements.Count == 0)
            return null;

        var ring = game.Rings.Find(r => r.Element == conflict.Elements[0] && r.ClaimedBy == actingPlayer);
        return ring is not null && role.Traits.Contains(ring.Element) ? ring : null;
    }
}
