using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// akodo-gunso: after this character enters play from one of its controller's province
/// slots, refill that slot faceup with the top card of the deck. Its own script trusts a
/// caller-supplied context.ProvinceSlot (the slot it entered play from - see the script's own
/// doc comment: Card.ProvinceSlot on gunso itself is already cleared by the time it's in
/// play). GameLoop.RunPlayWindow captures the slot before clearing it, right when a character
/// is played from a province, and passes it here.
/// </summary>
public static class AkodoGunsoFirer
{
    public static void FireIfLegal(GameState game, Card card, string provinceSlot)
    {
        if (card.Id != "akodo-gunso")
            return;

        var context = new AbilityContext { Game = game, Player = card.Controller, Source = card, ProvinceSlot = provinceSlot };
        new AkodoGunsoRefillProvinceOnEnteringFromProvince().Execute(context);
    }
}
