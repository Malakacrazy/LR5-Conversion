using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// Evaluates one card-schema.json ring predicate (a "ringCondition") against a Ring -
/// the ring-scoped sibling of PredicateEvaluator. Deliberately covers only the ops
/// exercised by know-the-world (the only ported card using "mode": "ring" targets so far):
/// "isController" and "not". Same fail-loud policy as PredicateEvaluator.
/// </summary>
public static class RingPredicateEvaluator
{
    public static bool Evaluate(JsonElement predicate, Ring ring, AbilityContext context)
    {
        var op = predicate.GetProperty("op").GetString()!;

        return op switch
        {
            "not" => !Evaluate(predicate.GetProperty("of"), ring, context),
            "isController" => EvaluateIsController(predicate.GetProperty("controller").GetString()!, ring, context),
            _ => throw new NotSupportedException($"RingPredicateEvaluator does not yet support op '{op}'.")
        };
    }

    /// <summary>ringteki ring.js's claimedBy comparison - "any" means claimed by either player, ignoring Contested (no "declare a conflict on this ring" mechanic exists to make that distinction meaningful yet).</summary>
    private static bool EvaluateIsController(string controller, Ring ring, AbilityContext context) => controller switch
    {
        "self" => ring.ClaimedBy == context.Player,
        "opponent" => ring.ClaimedBy == context.Game.Opponent(context.Player),
        "any" => ring.ClaimedBy is not null,
        _ => throw new NotSupportedException($"Unknown isController controller '{controller}'.")
    };
}
