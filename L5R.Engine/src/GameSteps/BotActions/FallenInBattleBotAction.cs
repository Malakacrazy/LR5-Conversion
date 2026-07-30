using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>fallen-in-battle: needs Conflict.Winner/SkillDifference, only settled by the post-resolution window. Targets an opponent participant (the more aggressive of the two options the script's own permissive range allows).</summary>
public sealed class FallenInBattleBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict
        && conflict.Winner == actingPlayer
        && conflict.ConflictType == "military"
        && conflict.SkillDifference >= 5
        && FindTarget(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new FallenInBattleDiscardOnDecisiveMilitaryWin().Execute(context);
    }

    private static Card? FindTarget(GameState game, Player actingPlayer)
    {
        var conflict = game.CurrentConflict!;
        var opponent = game.Opponent(actingPlayer);
        return conflict.Attackers.Concat(conflict.Defenders).FirstOrDefault(c => c.Controller == opponent);
    }
}
