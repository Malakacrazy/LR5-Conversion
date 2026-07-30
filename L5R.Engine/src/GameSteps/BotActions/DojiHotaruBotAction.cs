using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Same shape as AkodoToturiBotAction, gated on a political conflict instead.</summary>
public sealed class DojiHotaruBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict
        && conflict.ConflictType == "political"
        && conflict.RingClaimedThisConflict
        && (conflict.Attackers.Contains(source) || conflict.Defenders.Contains(source))
        && FindClaimedRing(game, conflict, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var conflict = game.CurrentConflict!;
        var ring = FindClaimedRing(game, conflict, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot ring.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, TargetRing = ring };
        new DojiHotaruResolveRingOnClaimDuringPolitical().Execute(context);
    }

    private static Ring? FindClaimedRing(GameState game, Conflict conflict, Player actingPlayer) =>
        conflict.Elements.Count > 0
            ? game.Rings.Find(r => r.Element == conflict.Elements[0] && r.ClaimedBy == actingPlayer)
            : null;
}
