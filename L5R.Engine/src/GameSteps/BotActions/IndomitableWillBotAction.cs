using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// indomitable-will: needs Conflict.Winner, only settled by the post-resolution window - the
/// same shape as the earlier conflict-outcome-gated wave. No target needed (the script finds
/// its own sole participant).
/// </summary>
public sealed class IndomitableWillBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict
        && conflict.Winner == actingPlayer
        && conflict.Attackers.Concat(conflict.Defenders).Count(c => c.Controller == actingPlayer) == 1;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source };
        new IndomitableWillPreventBowOnSoloWin().Execute(context);
    }
}
