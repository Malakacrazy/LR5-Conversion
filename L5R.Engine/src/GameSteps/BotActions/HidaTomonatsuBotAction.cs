using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>hida-tomonatsu: Conflict.Winner is settled by the post-resolution window. Targets the first non-unique attacker.</summary>
public sealed class HidaTomonatsuBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict
        && conflict.Winner == actingPlayer
        && conflict.Defenders.Contains(source)
        && FindTarget(conflict) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game.CurrentConflict!)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new HidaTomonatsuReturnAttackerToDeckOnDefendedWin().Execute(context);
    }

    private static Card? FindTarget(Conflict conflict) =>
        conflict.Attackers.FirstOrDefault(c => !c.Unique);
}
