using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// Resolves the legal-targets set for a TargetDefinition: cardType + controller filters,
/// plus cardCondition via PredicateEvaluator.
/// </summary>
public static class TargetResolver
{
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
