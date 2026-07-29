using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// intimidating-hida: after the opponent passes on declaring a conflict as the attacking
/// player, that player loses 1 honor ("event.conflict.attackingPlayer === context.player.
/// opponent"). A "pass" isn't otherwise modeled in this engine (no conflict is actually
/// declared), so the caller represents "the opponent would have been the attacking player"
/// via an ordinary Conflict object carrying just that fact - the same caller-supplies-the-
/// fact convention Conflict.Winner/Loser already use for afterConflict reactions. The effect
/// reuses LoseHonorGameActionHandler directly (already targets context.Game.Opponent(context.
/// Player)).
/// </summary>
public sealed class IntimidatingHidaLoseHonorOnOpponentPass : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var hida = context.Source;
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{hida.Id}' requires an active conflict.");

        if (conflict.AttackingPlayer != context.Game.Opponent(context.Player))
            throw new InvalidOperationException($"'{hida.Id}' can only trigger when the opponent passes as the attacking player.");

        new LoseHonorGameActionHandler().Execute(context, null);
    }
}
