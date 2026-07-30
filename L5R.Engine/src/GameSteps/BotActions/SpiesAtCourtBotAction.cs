using System;
using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>spies-at-court: needs Conflict.Winner, only settled by the post-resolution window. Dishonors a non-dishonored own participant as the cost.</summary>
public sealed class SpiesAtCourtBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict
        && conflict.Winner == actingPlayer
        && conflict.ConflictType == "political"
        && FindCostTarget(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var costTarget = FindCostTarget(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot cost target.");

        var opponent = game.Opponent(actingPlayer);
        var discarded = opponent.Hand.Take(Math.Min(2, opponent.Hand.Count)).ToList();

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, CostTarget = costTarget, ChosenDiscardCards = discarded };
        new SpiesAtCourtDiscardTwoOnPoliticalWin().Execute(context);
    }

    private static Card? FindCostTarget(GameState game, Player actingPlayer)
    {
        var conflict = game.CurrentConflict!;
        return conflict.Attackers.Concat(conflict.Defenders).FirstOrDefault(c => c.Controller == actingPlayer && !c.IsDishonored);
    }
}
