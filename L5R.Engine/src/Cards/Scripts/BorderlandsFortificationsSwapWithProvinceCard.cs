using L5R.Engine.Abilities;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// borderlands-fortifications: choose a card in one of your provinces and switch this
/// holding's own province slot with it. Needs a bespoke location-swap handler - no
/// gameAction models exchanging two cards' locations. Reuses Card.ProvinceSlot (see its
/// own doc comment) rather than moving either card between zones - both stay in
/// Player.Provinces throughout, only which slot each occupies changes.
/// </summary>
public sealed class BorderlandsFortificationsSwapWithProvinceCard : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var fortifications = context.Source;

        var target = context.Target
            ?? throw new InvalidOperationException($"'{fortifications.Id}' requires context.Target (the other province card) to be set.");

        if (target == fortifications)
            throw new InvalidOperationException($"'{fortifications.Id}' must swap with a different card.");

        if (target.Controller != context.Player)
            throw new InvalidOperationException($"'{target.Id}' must be controlled by '{fortifications.Id}''s controller.");

        if (!context.Player.Provinces.Contains(target))
            throw new InvalidOperationException($"'{target.Id}' must be in one of its controller's provinces.");

        if (!context.Player.Provinces.Contains(fortifications))
            throw new InvalidOperationException($"'{fortifications.Id}' must itself be in one of its controller's provinces.");

        (fortifications.ProvinceSlot, target.ProvinceSlot) = (target.ProvinceSlot, fortifications.ProvinceSlot);
    }
}
