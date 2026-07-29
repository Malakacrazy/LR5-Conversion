using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// spies-at-court: after winning a political conflict, dishonor a participating character
/// its controller controls to discard 2 cards from the opponent's hand. Needs Conflict.
/// Winner/ConflictType field inspection beyond the closed predicate vocabulary. The
/// dishonor cost's target is a caller-set fact (context.CostTarget, same convention as
/// every other parameterized cost this session), and "at random" reuses
/// ChosenDiscardGameActionHandler directly (context.ChosenDiscardCards supplies the
/// outcome, same as every other no-RNG-primitive convention this engine already has).
/// "max: perConflict(1)" needs no work, matching every other "max"/"limit" field's
/// established no-op precedent.
/// </summary>
public sealed class SpiesAtCourtDiscardTwoOnPoliticalWin : ICardScript
{
    private static readonly JsonElement AmountTwo = JsonDocument.Parse("{\"amount\":2}").RootElement;

    public void Execute(AbilityContext context)
    {
        var spiesAtCourt = context.Source;

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{spiesAtCourt.Id}' requires an active conflict.");

        if (conflict.Winner != context.Player)
            throw new InvalidOperationException($"'{spiesAtCourt.Id}' can only trigger when its controller wins the conflict.");

        if (conflict.ConflictType != "political")
            throw new InvalidOperationException($"'{spiesAtCourt.Id}' can only trigger after a political conflict.");

        var costTarget = context.CostTarget
            ?? throw new InvalidOperationException($"'{spiesAtCourt.Id}' requires context.CostTarget (the character to dishonor as a cost) to be set.");

        if (costTarget.Controller != context.Player)
            throw new InvalidOperationException($"'{costTarget.Id}' must be controlled by '{spiesAtCourt.Id}''s controller.");

        if (!IsParticipating(context.Game, costTarget))
            throw new InvalidOperationException($"'{costTarget.Id}' is not participating.");

        if (costTarget.IsDishonored)
            throw new InvalidOperationException($"'{costTarget.Id}' is already dishonored.");

        context.Target = costTarget;
        new DishonorGameActionHandler().Execute(context, null);

        new ChosenDiscardGameActionHandler().Execute(context, AmountTwo);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
