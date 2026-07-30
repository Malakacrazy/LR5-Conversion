using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// keeper-initiate: after claiming a ring matching your role's element, put this character
/// into play for free from your provinces or discard pile. Its own script requires being in
/// Provinces or Discard (unlike seeker-initiate, adopted via the normal
/// ChooseScriptedAction scan) - GameState.AllCards() is Hand+PlayArea only, so this card is
/// structurally invisible to that scan regardless of location. Fired directly here, scanning
/// Player.Provinces/Discard explicitly, at the same post-resolution moment KeeperOfRoleFirer
/// already checks (right after ring-claim is settled) - same "known hook site, explicit
/// scan" pattern as every other role/province card adopted this session.
/// </summary>
public static class KeeperInitiateFirer
{
    public static void FireIfLegal(GameState game, Player player)
    {
        if (player.Role is not { } role)
            return;

        if (game.CurrentConflict is not { RingClaimedThisConflict: true } conflict || conflict.Elements.Count == 0)
            return;

        var ring = game.Rings.Find(r => r.Element == conflict.Elements[0] && r.ClaimedBy == player);
        if (ring is null || !role.Traits.Contains(ring.Element))
            return;

        var keeperInitiate = player.Provinces.Concat(player.Discard).FirstOrDefault(c => c.Id == "keeper-initiate");
        if (keeperInitiate is null)
            return;

        var context = new AbilityContext { Game = game, Player = player, Source = keeperInitiate, TargetRing = ring };
        new KeeperInitiatePutIntoPlayOnMatchingRingClaim().Execute(context);
    }
}
