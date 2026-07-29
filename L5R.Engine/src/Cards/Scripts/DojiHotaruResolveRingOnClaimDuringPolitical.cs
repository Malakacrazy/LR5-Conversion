using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>doji-hotaru: same shape as AkodoToturiResolveRingOnClaimDuringMilitary, gated on a political conflict instead.</summary>
public sealed class DojiHotaruResolveRingOnClaimDuringPolitical : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var hotaru = context.Source;

        if (!context.Game.ConflictHasType("political"))
            throw new InvalidOperationException($"'{hotaru.Id}' can only trigger during a political conflict.");

        if (!IsParticipating(context.Game, hotaru))
            throw new InvalidOperationException($"'{hotaru.Id}' can only trigger while participating.");

        var ring = context.TargetRing
            ?? throw new InvalidOperationException($"'{hotaru.Id}' requires context.TargetRing (the ring just claimed) to be set.");

        if (ring.ClaimedBy != context.Player)
            throw new InvalidOperationException($"'{hotaru.Id}' can only trigger when its controller claims the ring.");

        new ResolveConflictRingGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
