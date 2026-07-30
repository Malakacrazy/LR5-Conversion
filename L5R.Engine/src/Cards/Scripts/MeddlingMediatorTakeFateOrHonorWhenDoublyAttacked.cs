using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// meddling-mediator: during the conflict phase, if the opponent has declared more than 1
/// conflict this phase (not counting passes), take 1 fate or 1 honor from them. Needs a
/// count of the opponent's not-passed conflict declarations this phase, a conflict-
/// collection query beyond the closed predicate vocabulary (see GameState.
/// ConflictDeclarationsThisPhase's own doc comment). context.ChosenChoice dispatches to
/// the existing TakeFateGameActionHandler/TakeHonorGameActionHandler, same convention as
/// asako-diplomat/ide-trader.
/// </summary>
public sealed class MeddlingMediatorTakeFateOrHonorWhenDoublyAttacked : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var mediator = context.Source;

        if (context.Game.CurrentPhase != Phase.Conflict)
            throw new InvalidOperationException($"'{mediator.Id}' can only be used during the conflict phase.");

        var opponent = context.Game.Opponent(context.Player);
        var declarations = context.Game.ConflictDeclarationsThisPhase.Count(d => d.Player == opponent && !d.Passed);
        if (declarations <= 1)
            throw new InvalidOperationException($"'{mediator.Id}' requires the opponent to have declared more than 1 conflict this phase.");

        switch (context.ChosenChoice)
        {
            case "Take 1 fate":
                new TakeFateGameActionHandler().Execute(context, null);
                break;
            case "Take 1 honor":
                new TakeHonorGameActionHandler().Execute(context, null);
                break;
            default:
                throw new InvalidOperationException($"'{mediator.Id}' requires context.ChosenChoice to be 'Take 1 fate' or 'Take 1 honor'.");
        }
    }
}
