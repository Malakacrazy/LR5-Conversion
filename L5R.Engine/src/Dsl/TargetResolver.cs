using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// Resolves the legal-targets set for a TargetDefinition: cardType + controller filters,
/// plus cardCondition via PredicateEvaluator.
/// </summary>
public static class TargetResolver
{
    /// <summary>
    /// Resolves a gameActionEntry's own "target" override, either the "allCardsMatching"
    /// valueRef shape (card-schema.json) - a bulk target, e.g. the-art-of-peace's "every
    /// attacking character" - or a "contextPath" resolving to a single card (court-mask's
    /// "dishonor the character this is attached to": {"contextPath": "source.parent"}) or a
    /// whole card list (kitsuki-investigator's "player.opponent.hand"), delegated to
    /// ValueRefResolver since it already understands both shapes. Other valueRef shapes on a
    /// gameAction's target throw.
    /// </summary>
    public static IReadOnlyList<Card> ResolveAllCardsMatching(JsonElement targetValueRef, AbilityContext context)
    {
        if (targetValueRef.TryGetProperty("contextPath", out _))
        {
            return ValueRefResolver.Resolve(targetValueRef, context) switch
            {
                Card card => new[] { card },
                IReadOnlyList<Card> cards => cards,
                _ => throw new InvalidOperationException("gameAction target 'contextPath' did not resolve to a card or card list.")
            };
        }

        if (!targetValueRef.TryGetProperty("allCardsMatching", out var allCardsMatching))
            throw new NotSupportedException("Only the 'allCardsMatching' and 'contextPath' valueRef shapes are supported as a gameAction's own target override so far.");

        var controller = allCardsMatching.TryGetProperty("controller", out var c) ? c.GetString()! : "any";
        var location = allCardsMatching.TryGetProperty("location", out var l) ? l.GetString()! : "play area";

        IEnumerable<Card> candidates = context.Game.AllCards().Where(card => card.Location == location);
        candidates = controller switch
        {
            "self" => candidates.Where(card => card.Controller == context.Player),
            "opponent" => candidates.Where(card => card.Controller != context.Player),
            "any" => candidates,
            _ => throw new NotSupportedException($"Unknown allCardsMatching controller '{controller}'.")
        };

        if (allCardsMatching.TryGetProperty("of", out var of))
            candidates = candidates.Where(card => PredicateEvaluator.Evaluate(of, card, context));

        return candidates.ToList();
    }

    /// <summary>
    /// GameState.AllCards() only concatenates Hand+PlayArea for both players - deliberately,
    /// since it feeds the persistentEffects/whileAttached board-wide scans and dynamic
    /// counts, which should never see facedown-in-province cards as "in play". A target
    /// whose Location includes "province" (daidoji-nerishma/staging-ground/iuchi-wayfinder,
    /// possibly alongside other locations like ambush's ["hand", "province"]) needs
    /// Provinces added to the pool instead of widening AllCards() itself and risking
    /// exposing province cards to those unrelated scans.
    /// </summary>
    public static IReadOnlyList<Card> ResolveLegalTargets(TargetDefinition target, AbilityContext context)
    {
        IEnumerable<Card> candidates = context.Game.AllCards();
        if (target.Location?.Contains("province") == true)
            candidates = candidates.Concat(context.Game.Player1.Provinces).Concat(context.Game.Player2.Provinces);

        if (target.CardType is not null)
        {
            var cardType = CardTypes.Parse(target.CardType);
            candidates = candidates.Where(card => card.Type == cardType);
        }

        candidates = target.Controller switch
        {
            "self" => candidates.Where(card => card.Controller == context.Player),
            "opponent" => candidates.Where(card => card.Controller != context.Player),
            "any" => candidates,
            _ => throw new NotSupportedException($"Unknown target controller '{target.Controller}'.")
        };

        if (target.Location is not null)
            candidates = candidates.Where(card => target.Location.Contains(card.Location));

        if (target.CardCondition is not null)
            candidates = candidates.Where(card => PredicateEvaluator.Evaluate(target.CardCondition.Value, card, context));

        return candidates.ToList();
    }
}
