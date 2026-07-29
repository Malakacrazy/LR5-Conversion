using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// Evaluates one card-schema.json predicate node against a candidate card. Deliberately
/// covers only the ops exercised by cards in the executable set so far - an unrecognized
/// op throws rather than silently evaluating to true/false, since a predicate the
/// interpreter can't actually check must never be treated as "passed".
/// </summary>
public static class PredicateEvaluator
{
    public static bool Evaluate(JsonElement predicate, Card candidate, AbilityContext context)
    {
        var op = predicate.GetProperty("op").GetString()!;

        return op switch
        {
            "true" => true,
            "and" => predicate.GetProperty("of").EnumerateArray().All(p => Evaluate(p, candidate, context)),
            "or" => predicate.GetProperty("of").EnumerateArray().Any(p => Evaluate(p, candidate, context)),
            "not" => !Evaluate(predicate.GetProperty("of"), candidate, context),
            "isSelf" => candidate == context.Source,
            "isType" => candidate.Type == CardTypes.Parse(predicate.GetProperty("type").GetString()!),
            "hasTrait" => candidate.Traits.Contains(predicate.GetProperty("trait").GetString()!),
            "hasFaction" => candidate.Faction == predicate.GetProperty("faction").GetString(),
            "hasStatus" => EvaluateHasStatus(predicate.GetProperty("status").GetString()!, candidate, context),
            "compareStat" => EvaluateCompareStat(predicate, candidate, context),
            "compareValues" => Compare(
                ValueRefResolver.ResolveInt(predicate.GetProperty("left"), context),
                predicate.GetProperty("comparator").GetString()!,
                ValueRefResolver.ResolveInt(predicate.GetProperty("right"), context)),
            "isDuringConflict" => EvaluateIsDuringConflict(predicate, context),
            "anyCardMatches" => EvaluateAnyCardMatches(predicate, context),
            _ => throw new NotSupportedException($"PredicateEvaluator does not yet support op '{op}'.")
        };
    }

    /// <summary>An existential check over a card scope - same controller/location/of filters as TargetResolver.ResolveAllCardsMatching, but Any() instead of collecting the list.</summary>
    private static bool EvaluateAnyCardMatches(JsonElement predicate, AbilityContext context)
    {
        var controller = predicate.TryGetProperty("controller", out var c) ? c.GetString()! : "any";
        var location = predicate.TryGetProperty("location", out var l) ? l.GetString()! : "play area";

        IEnumerable<Card> candidates = context.Game.AllCards().Where(card => card.Location == location);
        candidates = controller switch
        {
            "self" => candidates.Where(card => card.Controller == context.Player),
            "opponent" => candidates.Where(card => card.Controller != context.Player),
            "any" => candidates,
            _ => throw new NotSupportedException($"Unknown anyCardMatches controller '{controller}'.")
        };

        var of = predicate.GetProperty("of");
        return candidates.Any(card => Evaluate(of, card, context));
    }

    /// <summary>
    /// ringteki isDuringConflict(types): with no "type", just checks game.currentConflict
    /// exists - here, that we're in the conflict phase. With a "type" (military/political,
    /// or an air/earth/fire/water/void ring element - card-schema.json's single enum,
    /// since ringteki checks conflictType.concat(elements) against the same list), it also
    /// requires an actual Conflict and matches against its ConflictType or Elements. No
    /// conflict yet declared is a real (false) answer, matching the no-conflict-is-false
    /// convention used by hasStatus's isParticipating/isAttacking/isDefending.
    /// </summary>
    private static bool EvaluateIsDuringConflict(JsonElement predicate, AbilityContext context)
    {
        if (context.Game.CurrentPhase != Phase.Conflict)
            return false;

        if (!predicate.TryGetProperty("type", out var typeElement))
            return true;

        var type = typeElement.GetString()!;
        return context.Game.CurrentConflict is { } conflict
            && (type == conflict.ConflictType || conflict.Elements.Contains(type));
    }

    /// <summary>
    /// ringteki BaseCard.isParticipating()/isAttacking()/isDefending() all start with
    /// `this.game.currentConflict &&` - outside a conflict they're simply false, not an
    /// unsupported case, so no-conflict is a real (false) answer rather than a throw.
    /// </summary>
    private static bool EvaluateHasStatus(string status, Card candidate, AbilityContext context) => status switch
    {
        "isBowed" => candidate.Bowed,
        "isUnique" => candidate.Unique,
        "isHonored" => candidate.IsHonored,
        "isDishonored" => candidate.IsDishonored,
        "isParticipating" => context.Game.CurrentConflict is { } conflict
            && (conflict.Attackers.Contains(candidate) || conflict.Defenders.Contains(candidate)),
        "isAttacking" => context.Game.CurrentConflict?.Attackers.Contains(candidate) == true,
        "isDefending" => context.Game.CurrentConflict?.Defenders.Contains(candidate) == true,
        _ => throw new NotSupportedException($"PredicateEvaluator does not yet support hasStatus '{status}'.")
    };

    private static bool EvaluateCompareStat(JsonElement predicate, Card candidate, AbilityContext context)
    {
        var stat = predicate.GetProperty("stat").GetString()!;
        var comparator = predicate.GetProperty("comparator").GetString()!;
        var value = ValueRefResolver.ResolveInt(predicate.GetProperty("value"), context);

        var candidateValue = stat switch
        {
            "printedCost" => candidate.PrintedCost
                ?? throw new InvalidOperationException($"Card '{candidate.Id}' has no printedCost to compare."),
            _ => throw new NotSupportedException($"PredicateEvaluator does not yet support compareStat stat '{stat}'.")
        };

        return Compare(candidateValue, comparator, value);
    }

    internal static bool Compare(int left, string comparator, int right) => comparator switch
    {
        "lt" => left < right,
        "lte" => left <= right,
        "eq" => left == right,
        "gte" => left >= right,
        "gt" => left > right,
        _ => throw new NotSupportedException($"Unknown comparator '{comparator}'.")
    };
}
