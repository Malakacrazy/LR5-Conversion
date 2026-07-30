using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// rebuild: shuffle a card in one of your unbroken provinces back into your deck (the
/// cost), then put a holding from your discard pile into play in that same province.
/// Needs a candidate-relative province-status lookup (Card.Broken - see its own doc
/// comment) and a cost-to-target cross-reference (which slot opened up), neither modeled
/// by the closed vocabulary. "Shuffle" is a verified no-op (no RNG/shuffle primitive
/// exists - DeckSearchGameActionHandler's own doc comment).
/// </summary>
public sealed class RebuildReplaceProvinceCardWithHolding : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var rebuild = context.Source;

        var costTarget = context.CostTarget
            ?? throw new InvalidOperationException($"'{rebuild.Id}' requires context.CostTarget (the unbroken province card to shuffle away) to be set.");

        if (costTarget.Controller != context.Player)
            throw new InvalidOperationException($"'{costTarget.Id}' must be controlled by '{rebuild.Id}''s controller.");

        if (!context.Player.Provinces.Contains(costTarget))
            throw new InvalidOperationException($"'{costTarget.Id}' must be in one of its controller's provinces.");

        if (costTarget.Broken)
            throw new InvalidOperationException($"'{costTarget.Id}' must not be broken.");

        var slot = costTarget.ProvinceSlot
            ?? throw new InvalidOperationException($"'{costTarget.Id}' requires ProvinceSlot to be set.");

        var holding = context.Target
            ?? throw new InvalidOperationException($"'{rebuild.Id}' requires context.Target (the holding to put into play) to be set.");

        if (holding.Controller != context.Player)
            throw new InvalidOperationException($"'{holding.Id}' must be controlled by '{rebuild.Id}''s controller.");

        if (holding.Type != CardType.Holding)
            throw new InvalidOperationException($"'{holding.Id}' must be a holding.");

        if (holding.Location != "discard")
            throw new InvalidOperationException($"'{holding.Id}' must be in its controller's discard pile.");

        context.Player.Provinces.Remove(costTarget);
        ZoneMover.MoveTo(costTarget, context.Player.Deck, "deck");
        costTarget.ProvinceSlot = null;

        ZoneMover.MoveTo(holding, context.Player.Provinces, "province");
        holding.ProvinceSlot = slot;
        holding.Facedown = false;
    }
}
