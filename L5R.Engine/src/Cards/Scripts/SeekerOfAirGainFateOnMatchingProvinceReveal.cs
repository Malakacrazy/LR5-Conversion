using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// seeker-of-air: after an air province its controller owns is revealed, gain 1 fate.
/// ringteki's own ProvinceCard.getElement() reads the province's elements straight off its
/// traits list (provinces have no separate "elements" runtime field) - same convention
/// used here, checking revealedProvince.Traits directly. The revealed province is a
/// caller-set fact (context.Target, trust-the-caller) since there's no reveal-event bus.
/// </summary>
public sealed class SeekerOfAirGainFateOnMatchingProvinceReveal : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var seeker = context.Source;

        var revealedProvince = context.Target
            ?? throw new InvalidOperationException($"'{seeker.Id}' requires context.Target (the revealed province) to be set.");

        if (revealedProvince.Controller != context.Player)
            throw new InvalidOperationException($"'{revealedProvince.Id}' must be controlled by '{seeker.Id}''s controller.");

        if (revealedProvince.Type != CardType.Province)
            throw new InvalidOperationException($"'{revealedProvince.Id}' must be a province.");

        if (!revealedProvince.Traits.Contains("air"))
            throw new InvalidOperationException($"'{revealedProvince.Id}' must be an air province.");

        new GainFateGameActionHandler().Execute(context, null);
    }
}
