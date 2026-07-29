using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// akodo-toturi: after claiming a ring during a military conflict this character is
/// participating in, resolve that ring's effect. Needs event.player field inspection
/// beyond the closed predicate vocabulary's event.card-only convention, since onClaimRing
/// events carry a player field. context.TargetRing carries the ring just claimed, same
/// caller-set-fact convention as seeker-initiate/keeper-initiate. Reuses the new
/// ResolveConflictRingGameActionHandler ("resolveConflictRing") directly.
/// </summary>
public sealed class AkodoToturiResolveRingOnClaimDuringMilitary : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var toturi = context.Source;

        if (!context.Game.ConflictHasType("military"))
            throw new InvalidOperationException($"'{toturi.Id}' can only trigger during a military conflict.");

        if (!IsParticipating(context.Game, toturi))
            throw new InvalidOperationException($"'{toturi.Id}' can only trigger while participating.");

        var ring = context.TargetRing
            ?? throw new InvalidOperationException($"'{toturi.Id}' requires context.TargetRing (the ring just claimed) to be set.");

        if (ring.ClaimedBy != context.Player)
            throw new InvalidOperationException($"'{toturi.Id}' can only trigger when its controller claims the ring.");

        new ResolveConflictRingGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
