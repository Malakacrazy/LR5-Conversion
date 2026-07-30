using System.Collections.Generic;
using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// seeker-of-air/earth/fire/void/water: after a province its controller owns, matching the
/// role's own element, is revealed, gain 1 fate. Same reasoning as KeeperOfRoleFirer -
/// checks each player's own Player.Role directly rather than a card scan, fired at the same
/// conflict-declaration/reveal moment as SecretCacheFirer and friends. Only the revealed
/// province's own controller's role can match (the script itself checks
/// revealedProvince.Controller == context.Player), so this only ever checks that one player.
/// </summary>
public static class SeekerOfRoleFirer
{
    private static readonly Dictionary<string, ICardScript> ScriptsByRoleId = new()
    {
        ["seeker-of-air"] = new SeekerOfAirGainFateOnMatchingProvinceReveal(),
        ["seeker-of-earth"] = new SeekerOfEarthGainFateOnMatchingProvinceReveal(),
        ["seeker-of-fire"] = new SeekerOfFireGainFateOnMatchingProvinceReveal(),
        ["seeker-of-void"] = new SeekerOfVoidGainFateOnMatchingProvinceReveal(),
        ["seeker-of-water"] = new SeekerOfWaterGainFateOnMatchingProvinceReveal()
    };

    public static void FireIfLegal(GameState game, Card revealedProvince)
    {
        var controller = revealedProvince.Controller;
        if (controller.Role is not { } role || !ScriptsByRoleId.TryGetValue(role.Id, out var script))
            return;

        // "seeker-of-air" -> "air": each script also throws if the province's own traits
        // don't include it, so this must be checked before invoking, not left to the script.
        var element = role.Id["seeker-of-".Length..];
        if (revealedProvince.Type != CardType.Province || !revealedProvince.Traits.Contains(element))
            return;

        var context = new AbilityContext { Game = game, Player = controller, Source = role, Target = revealedProvince };
        script.Execute(context);
    }
}
