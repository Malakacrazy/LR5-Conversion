using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// Resolves the legal-targets set for a TargetDefinition: cardType + controller filters
/// only so far. cardCondition is parsed but not evaluated - no predicate interpreter
/// exists yet, so a target that needs one throws rather than silently matching everyone.
/// </summary>
public static class TargetResolver
{
    public static IReadOnlyList<Card> ResolveLegalTargets(TargetDefinition target, AbilityContext context)
    {
        if (target.CardCondition is not null)
            throw new NotSupportedException("TargetResolver does not yet evaluate cardCondition predicates.");

        IEnumerable<Card> candidates = context.Game.AllCards();

        if (target.CardType is not null)
        {
            var cardType = ParseCardType(target.CardType);
            candidates = candidates.Where(card => card.Type == cardType);
        }

        candidates = target.Controller switch
        {
            "self" => candidates.Where(card => card.Controller == context.Player),
            "opponent" => candidates.Where(card => card.Controller != context.Player),
            "any" => candidates,
            _ => throw new NotSupportedException($"Unknown target controller '{target.Controller}'.")
        };

        return candidates.ToList();
    }

    private static CardType ParseCardType(string cardType) => cardType switch
    {
        "character" => CardType.Character,
        "holding" => CardType.Holding,
        "event" => CardType.Event,
        "attachment" => CardType.Attachment,
        "province" => CardType.Province,
        "stronghold" => CardType.Stronghold,
        "role" => CardType.Role,
        _ => throw new NotSupportedException($"Unknown cardType '{cardType}'.")
    };
}
