using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// yogo-hiroue: while participating, move a character to this conflict (Execute); then,
/// only after that conflict ends and only if this character's controller won it, may
/// dishonor the moved character (ResolveDishonorChoice). Ringteki's "sequential" combinator
/// here really chains one immediate step with a delayedEffect keyed on afterConflict - a
/// genuinely later moment, not modeled by ICardScript's single Execute entry point (every
/// other reactive script this session represents exactly one moment). ResolveDishonorChoice
/// is a plain method, not part of ICardScript - the caller invokes it directly once that
/// later moment is reached, the same trust-the-caller convention as every other script,
/// just split across two methods since this ability genuinely spans two moments. It takes
/// the finished Conflict directly (context.Game.CurrentConflict is already null by then,
/// cleared by GameState.EndConflict()) and the moved character directly (context.Target
/// from the first call isn't retained anywhere generic).
/// </summary>
public sealed class YogoHiroueMoveThenMayDishonorOnWin : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var hiroue = context.Source;

        if (!IsParticipating(context.Game, hiroue))
            throw new InvalidOperationException($"'{hiroue.Id}' can only be used while participating.");

        if (context.Target is null)
            throw new InvalidOperationException($"'{hiroue.Id}' requires context.Target to be set.");

        new MoveToConflictGameActionHandler().Execute(context, null);
    }

    public void ResolveDishonorChoice(AbilityContext context, Conflict conflict, bool choseToDishonor)
    {
        var hiroue = context.Source;

        if (conflict.Winner != context.Player)
            throw new InvalidOperationException($"'{hiroue.Id}' can only offer this choice when its controller won the conflict.");

        if (!choseToDishonor)
            return;

        if (context.Target is null)
            throw new InvalidOperationException($"'{hiroue.Id}' requires context.Target (the character moved earlier) to be set.");

        new DishonorGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
