using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// asako-diplomat: Conflict.Winner is finally a stable, currently-queryable fact once the
/// post-resolution window runs (unlike the earlier mid-conflict window, where it's always
/// null). Prefers dishonoring an opponent's participant (the more aggressive of the two
/// options); falls back to honoring its own controller if no opponent participant exists.
/// </summary>
public sealed class AsakoDiplomatBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict
        && conflict.Winner == actingPlayer
        && (conflict.Attackers.Contains(source) || conflict.Defenders.Contains(source))
        && FindTarget(game, actingPlayer, out _) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, actingPlayer, out var choice)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target, ChosenChoice = choice };
        new AsakoDiplomatHonorOrDishonorOnWin().Execute(context);
    }

    private static Card? FindTarget(GameState game, Player actingPlayer, out string choice)
    {
        var conflict = game.CurrentConflict!;
        var opponentParticipant = conflict.Attackers.Concat(conflict.Defenders).FirstOrDefault(c => c.Controller == game.Opponent(actingPlayer));
        if (opponentParticipant is not null)
        {
            choice = "Dishonor this character";
            return opponentParticipant;
        }

        choice = "Honor this character";
        return actingPlayer.PlayArea.FirstOrDefault(c => c.Type == CardType.Character);
    }
}
