using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// hida-kisada: while in play and unblanked, if the opponent hasn't won a conflict yet
/// this round, cancel the first action ability the opponent tries to use each conflict.
/// Needs raw event registration across multiple event/stage pairs plus mutable per-round
/// state tracking, far beyond a single triggeredAbility. GameState.ConflictRecord (see its
/// own doc comment) supplies "hasn't won a conflict this round"; GameState.
/// FirstActionCancelledThisConflict supplies "only the first action per conflict" - the
/// cancellation itself needs no new mechanism, matching reprieve/stand-your-ground's own
/// reasoning: the caller simply never invokes whatever the opponent's action would have
/// been, and instead invokes this script to represent that it was cancelled.
/// </summary>
public sealed class HidaKisadaCancelOpponentsFirstActionEachConflict : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var kisada = context.Source;

        if (context.Game.CurrentConflict is null)
            throw new InvalidOperationException($"'{kisada.Id}' requires an active conflict.");

        if (kisada.Location != "play area")
            throw new InvalidOperationException($"'{kisada.Id}' must be in play.");

        if (context.Game.IsBlanked(kisada))
            throw new InvalidOperationException($"'{kisada.Id}' cannot trigger while blanked.");

        var opponent = context.Game.Opponent(context.Player);
        if (context.Game.ConflictRecord.Any(c => c.Winner == opponent))
            throw new InvalidOperationException($"'{kisada.Id}' can only trigger while the opponent hasn't won a conflict this round.");

        if (context.Game.FirstActionCancelledThisConflict)
            throw new InvalidOperationException($"'{kisada.Id}' has already cancelled the opponent's first action this conflict.");

        context.Game.FirstActionCancelledThisConflict = true;
    }
}
