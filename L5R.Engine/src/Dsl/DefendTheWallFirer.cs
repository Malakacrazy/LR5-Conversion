using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// defend-the-wall: after a conflict declared against this province ends, if its controller
/// won, resolve the ring effect. Needs Conflict.Winner, only settled once
/// ConflictResolver.Resolve finishes computing skill - fired at the post-resolution moment,
/// after ring-claim, same timing as the JSON conflict-outcome-gated reactions
/// (akodo-toturi/doji-hotaru). A province, never scanned by ChooseScriptedAction, so this is
/// a direct hook rather than a registry entry.
/// </summary>
public static class DefendTheWallFirer
{
    public static void FireIfLegal(GameState game, Card province)
    {
        if (province.Id != "defend-the-wall" || game.CurrentConflict is not { } conflict || conflict.DeclaredProvince != province)
            return;

        if (conflict.Winner != province.Controller)
            return;

        var context = new AbilityContext { Game = game, Player = province.Controller, Source = province };
        new DefendTheWallResolveRingAsAttacker().Execute(context);
    }
}
