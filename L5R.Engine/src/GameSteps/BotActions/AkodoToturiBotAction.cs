using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// akodo-toturi's own script checks ring.ClaimedBy == context.Player, which isn't enough on
/// its own - ring.ClaimedBy persists across conflicts once set, so it would still read true
/// in a later, unrelated conflict. The adapter adds Conflict.RingClaimedThisConflict (set by
/// ConflictResolver only when this specific conflict just transitioned the ring from
/// unclaimed to claimed) so the action only fires on the conflict that actually earned it.
/// Legal only during the post-resolution window, once ring claim has actually happened.
/// </summary>
public sealed class AkodoToturiBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict
        && conflict.ConflictType == "military"
        && conflict.RingClaimedThisConflict
        && (conflict.Attackers.Contains(source) || conflict.Defenders.Contains(source))
        && FindClaimedRing(game, conflict, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var conflict = game.CurrentConflict!;
        var ring = FindClaimedRing(game, conflict, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot ring.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, TargetRing = ring };
        new AkodoToturiResolveRingOnClaimDuringMilitary().Execute(context);
    }

    private static Ring? FindClaimedRing(GameState game, Conflict conflict, Player actingPlayer) =>
        conflict.Elements.Count > 0
            ? game.Rings.Find(r => r.Element == conflict.Elements[0] && r.ClaimedBy == actingPlayer)
            : null;
}
