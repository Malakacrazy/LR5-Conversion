using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for bayushi-shoju. Target pool is any opponent-controlled participant in the current political conflict; picks the first one.</summary>
public sealed class BayushiShojuBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { ConflictType: "political" } conflict
        && (conflict.Attackers.Contains(source) || conflict.Defenders.Contains(source))
        && FindTarget(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new BayushiShojuReducePoliticalSkillWithDeathCheck().Execute(context);
    }

    private static Card? FindTarget(GameState game, Player actingPlayer)
    {
        var conflict = game.CurrentConflict;
        if (conflict is null)
            return null;

        var opponent = game.Opponent(actingPlayer);
        return conflict.Attackers.Concat(conflict.Defenders).FirstOrDefault(c => c.Controller == opponent);
    }
}
