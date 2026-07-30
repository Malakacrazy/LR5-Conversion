using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>court-games: during a political conflict. Prefers dishonoring an opponent's participant (the more aggressive option); falls back to honoring its own.</summary>
public sealed class CourtGamesBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { ConflictType: "political" }
        && FindTarget(game, actingPlayer, out _) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, actingPlayer, out var choice)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target, ChosenChoice = choice };
        new CourtGamesHonorOrDishonorParticipant().Execute(context);
    }

    private static Card? FindTarget(GameState game, Player actingPlayer, out string choice)
    {
        var conflict = game.CurrentConflict!;
        var opponent = game.Opponent(actingPlayer);
        var opponentParticipant = conflict.Attackers.Concat(conflict.Defenders).FirstOrDefault(c => c.Controller == opponent);
        if (opponentParticipant is not null)
        {
            choice = "Dishonor an opposing character";
            return opponentParticipant;
        }

        choice = "Honor a friendly character";
        return conflict.Attackers.Concat(conflict.Defenders).FirstOrDefault(c => c.Controller == actingPlayer);
    }
}
