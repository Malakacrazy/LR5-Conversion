using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// Resolves one card-schema.json valueRef to a literal value. Most valueRefs (numeric
/// dynamics, compareStat/cardLastingEffect's "value") are ints - ResolveInt is the
/// long-standing entry point for those and throws if the resolved value isn't one. A few
/// (contextPath's "imperialFavor" segment, a bare string literal like compareValues'
/// right: "") resolve to strings instead - Resolve returns the untyped value so
/// PredicateEvaluator's compareValues can dispatch on whichever type both sides turn out
/// to be. Deliberately covers only plain literals, a handful of named dynamics (each with
/// optional multiplier/offset arithmetic applied uniformly), and a few contextPath segments
/// so far - anything else throws until a card in the executable set needs it, per the same
/// fail-loud policy as PredicateEvaluator.
/// </summary>
public static class ValueRefResolver
{
    public static int ResolveInt(JsonElement valueRef, AbilityContext context) =>
        Resolve(valueRef, context) is int result
            ? result
            : throw new InvalidOperationException($"Expected valueRef to resolve to an int.");

    public static object Resolve(JsonElement valueRef, AbilityContext context)
    {
        if (valueRef.ValueKind == JsonValueKind.Number)
            return valueRef.GetInt32();

        if (valueRef.ValueKind == JsonValueKind.String)
            return valueRef.GetString()!;

        if (valueRef.TryGetProperty("dynamic", out var dynamicElement))
            return ResolveDynamic(dynamicElement.GetString()!, valueRef, context);

        if (valueRef.TryGetProperty("contextPath", out var contextPathElement))
            return ResolveContextPath(contextPathElement.GetString()!, context);

        throw new NotSupportedException("ValueRefResolver only supports plain literals, 'dynamic', and 'contextPath' so far.");
    }

    private static int ResolveDynamic(string name, JsonElement valueRef, AbilityContext context)
    {
        var raw = name switch
        {
            "conflictParticipantCount" => ResolveConflictParticipantCount(valueRef, context),
            "countHoldingsInPlay" => ResolveForPlayer(valueRef, context).PlayArea.Count(c => c.Type == CardType.Holding),
            "countCardsMatching" => ResolveCountCardsMatching(valueRef, context),
            "countClaimedRings" => context.Game.Rings.Count(r => r.ClaimedBy == ResolveForPlayer(valueRef, context)),
            "countUnclaimedRings" => context.Game.Rings.Count(r => r.IsUnclaimed),
            "countCardsInHand" => ResolveForPlayer(valueRef, context).Hand.Count,
            "countFacedownProvinces" => ResolveForPlayer(valueRef, context).Provinces.Count(c => c.Facedown),
            _ => throw new NotSupportedException($"ValueRefResolver does not yet support dynamic '{name}'.")
        };

        // card-schema.json: multiplier (default 1) and offset (default 0) apply to any
        // dynamic uniformly - restoration-of-balance's "opponent's hand size minus 4" is
        // {dynamic: countCardsInHand, for: opponent, offset: -4} rather than a bespoke
        // "count down to N" dynamic. Can go negative (e.g. a hand of 2 minus 4) - it's each
        // dynamic's own consumer (ChosenDiscardGameActionHandler) that clamps to what
        // actually makes sense for it, not this general-purpose arithmetic step.
        var multiplier = valueRef.TryGetProperty("multiplier", out var m) ? m.GetInt32() : 1;
        var offset = valueRef.TryGetProperty("offset", out var o) ? o.GetInt32() : 0;
        return raw * multiplier + offset;
    }

    /// <summary>
    /// The counting equivalent of allCardsMatching/target's controller+of filter - schema
    /// defaults controller to "self" here (unlike target/allCardsMatching's "any"), since a
    /// bare count with no explicit subject reads naturally as "how many of my own cards".
    /// </summary>
    private static int ResolveCountCardsMatching(JsonElement valueRef, AbilityContext context)
    {
        var controller = valueRef.TryGetProperty("controller", out var c) ? c.GetString()! : "self";
        IEnumerable<Card> candidates = context.Game.AllCards();
        candidates = controller switch
        {
            "self" => candidates.Where(card => card.Controller == context.Player),
            "opponent" => candidates.Where(card => card.Controller != context.Player),
            "any" => candidates,
            _ => throw new NotSupportedException($"Unknown controller '{controller}'.")
        };

        if (valueRef.TryGetProperty("of", out var ofElement))
            candidates = candidates.Where(card => PredicateEvaluator.Evaluate(ofElement, card, context));

        return candidates.Count();
    }

    /// <summary>valueRef's "for" (self/opponent, default self) - resolved relative to context.Player.</summary>
    private static Player ResolveForPlayer(JsonElement valueRef, AbilityContext context)
    {
        var forWhom = valueRef.TryGetProperty("for", out var forElement) ? forElement.GetString() : "self";
        return forWhom switch
        {
            "self" => context.Player,
            "opponent" => context.Game.Opponent(context.Player),
            _ => throw new NotSupportedException($"Unknown 'for' value '{forWhom}'.")
        };
    }

    /// <summary>ringteki currentConflict.getNumberOfParticipantsFor(role) - "own" resolves to whichever role context.Source's controller currently holds.</summary>
    private static int ResolveConflictParticipantCount(JsonElement valueRef, AbilityContext context)
    {
        var conflict = context.Game.CurrentConflict;
        if (conflict is null)
            return 0;

        var role = valueRef.GetProperty("role").GetString()!;
        if (role == "own")
            role = context.Source.Controller == conflict.AttackingPlayer ? "attacker" : "defender";

        return role switch
        {
            "attacker" => conflict.Attackers.Count,
            "defender" => conflict.Defenders.Count,
            _ => throw new NotSupportedException($"Unknown conflict role '{role}'.")
        };
    }

    private static object ResolveContextPath(string path, AbilityContext context)
    {
        var segments = path.Split('.');

        object current = segments[0] switch
        {
            "player" => context.Player,
            "source" => context.Source,
            // soshi-illusionist's "target.personalHonor" - the ability's currently chosen
            // target (set by AbilityExecutor.Resolve before a target-scoped gameAction runs).
            "target" => context.Target
                ?? throw new InvalidOperationException("contextPath root 'target' requires context.Target to be set."),
            _ => throw new NotSupportedException($"ValueRefResolver does not yet support contextPath root '{segments[0]}'.")
        };

        for (var i = 1; i < segments.Length; i++)
        {
            current = (current, segments[i]) switch
            {
                (Player p, "honor") => p.Honor,
                (Player p, "showBid") => p.ShowBid,
                (Player p, "imperialFavor") => p.ImperialFavor,
                (Player p, "opponent") => context.Game.Opponent(p),
                // kitsuki-investigator's "player.opponent.hand" - returns the List<Card>
                // itself rather than a single card; TargetResolver.ResolveAllCardsMatching is
                // what actually consumes this shape (its contextPath branch accepts either a
                // single Card or a card list).
                (Player p, "hand") => p.Hand,
                // court-mask/favored-mount: "source.parent" - the character an attachment
                // (context.Source) is currently attached to. Only meaningful for an
                // attachment mid-resolution, so an unattached source is a real error, not
                // a null/no-op - matches ValueRefResolver's fail-loud policy elsewhere.
                (Card c, "parent") => c.AttachedTo
                    ?? throw new InvalidOperationException($"'{c.Id}' is not currently attached to anything."),
                // soshi-illusionist's "target.personalHonor" - ringteki's StatusToken is a
                // real object a card holds in one slot; this engine flattens honored/
                // dishonored onto Card.IsHonored/IsDishonored directly (see
                // DiscardStatusTokenGameActionHandler), so there is no separate token object
                // to resolve to - "the status-token slot on this card" and "this card" are
                // the same thing here. A deliberate passthrough, not a real property walk.
                (Card c, "personalHonor") => c,
                _ => throw new NotSupportedException($"ValueRefResolver does not yet support contextPath segment '{segments[i]}' on {current.GetType().Name}.")
            };
        }

        return current;
    }
}
