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
    /// Resolves a gameActionEntry's own "target" override when it's the "allCardsMatching"
    /// valueRef shape (card-schema.json) - a bulk target, e.g. the-art-of-peace's "every
    /// attacking character". Other valueRef shapes on a gameAction's target throw.
    /// </summary>
    public static IReadOnlyList<Card> ResolveAllCardsMatching(JsonElement targetValueRef, AbilityContext context)
    {
        if (!targetValueRef.TryGetProperty("allCardsMatching", out var allCardsMatching))
            throw new NotSupportedException("Only the 'allCardsMatching' valueRef shape is supported as a gameAction's own target override so far.");

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

    public static IReadOnlyList<Card> ResolveLegalTargets(TargetDefinition target, AbilityContext context)
    {
        IEnumerable<Card> candidates = context.Game.AllCards();

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

        if (target.CardCondition is not null)
            candidates = candidates.Where(card => PredicateEvaluator.Evaluate(target.CardCondition.Value, card, context));

        return candidates.ToList();
    }
}
