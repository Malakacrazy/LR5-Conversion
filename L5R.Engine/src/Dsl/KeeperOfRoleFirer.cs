using System.Collections.Generic;
using L5R.Engine.Abilities;
using L5R.Engine.Cards;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// keeper-of-air/earth/fire/void/water: after this player wins a conflict they're
/// defending, declared with the matching element, gain 1 fate. Role cards are never
/// scanned by ChooseScriptedAction (roles aren't in Hand/PlayArea, and none of these
/// scripts even check the caller's own identity beyond context.Player - the caller is
/// trusted to only invoke the right one), so this checks each player's own Player.Role
/// directly and fires the matching script, at the same post-resolution moment the JSON
/// conflict-outcome-gated reactions use (Conflict.Winner is only settled there).
/// </summary>
public static class KeeperOfRoleFirer
{
    private static readonly Dictionary<string, ICardScript> ScriptsByRoleId = new()
    {
        ["keeper-of-air"] = new KeeperOfAirGainFateOnDefendedWin(),
        ["keeper-of-earth"] = new KeeperOfEarthGainFateOnEarthDefenseWin(),
        ["keeper-of-fire"] = new KeeperOfFireGainFateOnFireDefenseWin(),
        ["keeper-of-void"] = new KeeperOfVoidGainFateOnVoidDefenseWin(),
        ["keeper-of-water"] = new KeeperOfWaterGainFateOnWaterDefenseWin()
    };

    public static void FireIfLegal(GameState game, Player player)
    {
        if (player.Role is not { } role || !ScriptsByRoleId.TryGetValue(role.Id, out var script))
            return;

        // "keeper-of-air" -> "air": each script also throws if the conflict's element
        // doesn't match, so this must be checked before invoking, not left to the script.
        var element = role.Id["keeper-of-".Length..];

        if (game.CurrentConflict is not { } conflict
            || conflict.Winner != player || conflict.DefendingPlayer != player || !conflict.Elements.Contains(element))
            return;

        var context = new AbilityContext { Game = game, Player = player, Source = role };
        script.Execute(context);
    }
}
