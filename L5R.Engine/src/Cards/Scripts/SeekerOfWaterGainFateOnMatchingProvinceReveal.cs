using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>seeker-of-water: same shape as SeekerOfAirGainFateOnMatchingProvinceReveal, gated on the water trait instead.</summary>
public sealed class SeekerOfWaterGainFateOnMatchingProvinceReveal : ICardScript
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

        if (!revealedProvince.Traits.Contains("water"))
            throw new InvalidOperationException($"'{revealedProvince.Id}' must be a water province.");

        new GainFateGameActionHandler().Execute(context, null);
    }
}
