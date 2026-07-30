using L5R.Engine.Abilities;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// akodo-gunso: after this character enters play from one of its controller's province
/// slots, refill that slot faceup with the top card of the deck. Needs event.
/// originalLocation array-membership inspection and referencing that field as a gameAction
/// param, neither modeled by the closed vocabulary. context.ProvinceSlot carries which
/// slot it entered play from (a zone-move-event snapshot this engine doesn't record
/// automatically - Card.ProvinceSlot on gunso itself is already cleared by the time it's
/// in play).
/// </summary>
public sealed class AkodoGunsoRefillProvinceOnEnteringFromProvince : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var gunso = context.Source;

        var slot = context.ProvinceSlot
            ?? throw new InvalidOperationException($"'{gunso.Id}' requires context.ProvinceSlot (the slot it entered play from) to be set.");

        var refill = context.Player.Deck.FirstOrDefault()
            ?? throw new InvalidOperationException($"'{context.Player.Name}' has no cards left in their deck to refill with.");

        context.Player.Deck.Remove(refill);
        context.Player.Provinces.Add(refill);
        refill.Location = "province";
        refill.ProvinceSlot = slot;
        refill.Facedown = false;
    }
}
