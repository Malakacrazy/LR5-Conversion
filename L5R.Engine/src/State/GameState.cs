using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;

namespace L5R.Engine.State;

public sealed class GameState
{
    public required Player Player1 { get; init; }
    public required Player Player2 { get; init; }
    public Phase CurrentPhase { get; set; }
    public required Player ActivePlayer { get; set; }

    /// <summary>
    /// ringteki game.js: roundNumber starts at 0 and is bumped to 1 by DynastyPhase.
    /// createPhase() the first time the game enters Dynasty. Our GameState is always
    /// constructed already "mid-game" (tests set CurrentPhase directly, with no separate
    /// game-start step), so 1 is the sensible resting default rather than a pre-game 0
    /// nothing in this engine represents yet.
    /// </summary>
    public int RoundNumber { get; set; } = 1;

    /// <summary>All cards controlled by either player, regardless of zone.</summary>
    public IEnumerable<Card> AllCards() => Player1.Hand.Concat(Player1.PlayArea).Concat(Player2.Hand).Concat(Player2.PlayArea);

    /// <summary>
    /// The 5 rings' persistent claimed state - see Ring's own doc comment. Defaults to the
    /// canonical unclaimed 5 (ringteki game.js's ring construction: air/fire/water=military,
    /// earth/void=political) so tests only need to override what they're actually exercising.
    /// </summary>
    public List<Ring> Rings { get; init; } = new()
    {
        new Ring { Element = "air", ConflictType = "military" },
        new Ring { Element = "earth", ConflictType = "political" },
        new Ring { Element = "fire", ConflictType = "military" },
        new Ring { Element = "water", ConflictType = "military" },
        new Ring { Element = "void", ConflictType = "political" }
    };

    public Player Opponent(Player player) => player == Player1 ? Player2 : Player1;

    /// <summary>Null outside of a conflict - see Conflict's own doc comment for what's deliberately not modeled yet.</summary>
    public Conflict? CurrentConflict { get; set; }

    /// <summary>
    /// Active cardLastingEffect modifiers - see CardLastingEffectGameActionHandler and
    /// LastingEffect's own doc comment for the two durations stored here and how each expires.
    /// </summary>
    public List<LastingEffect> LastingEffects { get; } = new();

    /// <summary>Active cardLastingEffect "addKeyword" grants - see LastingKeywordGrant's own doc comment for why this is a separate list from LastingEffects.</summary>
    public List<LastingKeywordGrant> LastingKeywordGrants { get; } = new();

    public int EffectiveGlory(Card card) => EffectiveStat(card, "glory", card.PrintedGlory);

    public int EffectiveMilitarySkill(Card card) =>
        EffectiveStat(card, "military", HasSwitchedBaseSkills(card) ? card.PrintedPoliticalSkill : card.PrintedMilitarySkill);

    public int EffectivePoliticalSkill(Card card) =>
        EffectiveStat(card, "political", HasSwitchedBaseSkills(card) ? card.PrintedMilitarySkill : card.PrintedPoliticalSkill);

    /// <summary>bayushi-yunako's switchBaseSkills - see CardLastingEffectGameActionHandler's own doc comment for why this reuses LastingEffects with a sentinel Stat name instead of a new list.</summary>
    private bool HasSwitchedBaseSkills(Card card) => LastingEffects.Any(e => e.Target == card && e.Stat == "switchBaseSkills");

    public int EffectiveProvinceStrength(Card card) => EffectiveStat(card, "provinceStrength", card.PrintedProvinceStrength);

    private int EffectiveStat(Card card, string stat, int? printedValue)
    {
        // way-of-the-lion's modifyBaseMilitarySkillMultiplier multiplies the printed base
        // before any additive delta is applied - see LastingEffect.Multiplier's own doc
        // comment. Every other LastingEffect entry has Multiplier == null and contributes
        // through Value exactly as before.
        var multiplier = LastingEffects
            .Where(e => e.Target == card && e.Stat == stat && e.Multiplier is not null)
            .Select(e => e.Multiplier!.Value)
            .DefaultIfEmpty(1)
            .Aggregate((a, b) => a * b);

        var total = (printedValue ?? 0) * multiplier
            + LastingEffects.Where(e => e.Target == card && e.Stat == stat && e.Multiplier is null).Sum(e => e.Value);

        total += SumStatDeltas(ActivePersistentEffectsAffecting(card), stat);
        total += SumStatDeltas(ActiveWhileAttachedEffectsFor(card), stat);

        return total;
    }

    /// <summary>
    /// Shared by both the persistentEffects and whileAttached scans. Checks
    /// EffectVocabulary.IsStatEffect *before* resolving "value" as an int - most scanned
    /// effects are irrelevant to whichever stat is being summed (cardCannot's value is an
    /// object, addTrait/addKeyword's is a string), and ValueRefResolver.ResolveInt would
    /// throw on those rather than just reporting "not a stat delta".
    /// </summary>
    private int SumStatDeltas(IEnumerable<(Card Source, JsonElement Effect)> pairs, string stat)
    {
        var total = 0;
        foreach (var (source, effect) in pairs)
        {
            var effectName = effect.GetProperty("name").GetString();
            if (!EffectVocabulary.IsStatEffect(effectName))
                continue;

            var value = effect.TryGetProperty("value", out var v) ? ValueRefResolver.ResolveInt(v, SourceContextFor(source)) : 0;
            if (EffectVocabulary.TryGetStatDeltas(effectName, value, out var deltas))
                total += deltas.Where(d => d.Stat == stat).Sum(d => d.Value);
        }

        return total;
    }

    /// <summary>
    /// Active cardCannot restrictions - see CardRestriction's own doc comment. Same two
    /// durations and expiry rules as LastingEffects (AdvancePhase clears everything,
    /// EndConflict only the untilEndOfConflict ones).
    /// </summary>
    public List<CardRestriction> Restrictions { get; } = new();

    /// <summary>Active takeControl effects - see ControlChange's own doc comment for why this is a direct-mutation-plus-explicit-revert mechanism rather than an on-demand sum like LastingEffects/Restrictions.</summary>
    public List<ControlChange> ControlChanges { get; } = new();

    /// <summary>ringteki effects.js takeControl: moves the card to newController's play area and records the original controller so EndConflict()/AdvancePhase() can revert it.</summary>
    public void TakeControl(Card card, Player newController, string duration)
    {
        var original = card.Controller;
        original.PlayArea.Remove(card);
        newController.PlayArea.Add(card);
        card.Controller = newController;

        ControlChanges.Add(new ControlChange { Target = card, OriginalController = original, Duration = duration });
    }

    private void RevertControlChanges(Func<ControlChange, bool> shouldRevert)
    {
        var toRevert = ControlChanges.Where(shouldRevert).ToList();
        foreach (var change in toRevert)
        {
            change.Target.Controller.PlayArea.Remove(change.Target);
            change.OriginalController.PlayArea.Add(change.Target);
            change.Target.Controller = change.OriginalController;
        }

        ControlChanges.RemoveAll(c => toRevert.Contains(c));
    }

    /// <summary>
    /// consideringCard is above-question's own need: "target" is restricted only against
    /// opponentsEvents, which - unlike every other restriction action checked so far (bow,
    /// declareAsAttacker/Defender, receiveDishonorToken, triggerAbilities) - depends on what
    /// is *attempting* the restricted action, not just whether card can do it at all. Null
    /// for every existing call site, matching the trust-the-caller convention (no target-
    /// legality pipeline exists to supply this automatically yet - see MatchesRestricts).
    /// </summary>
    public bool IsRestrictedFrom(Card card, string action, Card? consideringCard = null)
    {
        bool MatchesCondition(CardRestriction r) =>
            r.Condition is not { } condition || PredicateEvaluator.Evaluate(condition, card, SourceContextFor(card));

        if (Restrictions.Any(r => r.Target == card && r.Action == action && MatchesQualifier(r.Qualifier) && MatchesCondition(r)))
            return true;

        bool MatchesAction((Card Source, JsonElement Effect) pair)
        {
            var isRestriction = EffectVocabulary.TryGetRestrictionAction(
                pair.Effect.GetProperty("name").GetString(),
                pair.Effect.TryGetProperty("value", out var v) ? v : (JsonElement?)null,
                out var restrictedAction,
                out var qualifier,
                out var restricts);
            return isRestriction && restrictedAction == action && MatchesQualifier(qualifier) && MatchesRestricts(restricts, pair.Source.Controller, consideringCard);
        }

        return ActivePersistentEffectsAffecting(card).Any(MatchesAction)
            || ActiveWhileAttachedEffectsFor(card).Any(MatchesAction);
    }

    /// <summary>A restriction with no qualifier always applies; one with a qualifier (pacifism's "military") only applies during a conflict of that type/element - same check as isDuringConflict's "type" filter.</summary>
    private bool MatchesQualifier(string? qualifier) =>
        qualifier is null || (CurrentConflict is { } conflict && (qualifier == conflict.ConflictType || conflict.Elements.Contains(qualifier)));

    /// <summary>
    /// above-question's "opponentsEvents": true only when consideringCard is controlled by
    /// restrictionOwner's opponent (ringteki Restriction.js: context.player === effect.
    /// context.player.opponent) and is itself an event (ringteki: context.source.type ===
    /// CardTypes.Event). restrictionOwner is whoever controls the card that granted the
    /// restriction (above-question's controller), not `card`'s own controller.
    /// </summary>
    private bool MatchesRestricts(string? restricts, Player restrictionOwner, Card? consideringCard)
    {
        if (restricts is null)
            return true;

        if (consideringCard is null)
            return false;

        return restricts switch
        {
            "opponentsEvents" => consideringCard.Controller == Opponent(restrictionOwner) && consideringCard.Type == CardType.Event,
            // ringteki Restriction.js's opponentsCardEffects also checks context.source.type
            // is one of Event/Character/Holding/Attachment/Stronghold/Province/Role - exactly
            // CardType's full set, so every Card already satisfies it unconditionally here.
            "opponentsCardEffects" => consideringCard.Controller == Opponent(restrictionOwner),
            _ => throw new NotSupportedException($"Unknown cardCannot 'restricts' qualifier '{restricts}'.")
        };
    }

    /// <summary>
    /// Active playerCannot restrictions - see PlayerRestriction's own doc comment. Same two
    /// durations and expiry rules as Restrictions.
    /// </summary>
    public List<PlayerRestriction> PlayerRestrictions { get; } = new();

    /// <summary>Active reduceNextPlayedCardCost effects - see PlayerCostReduction's own doc comment.</summary>
    public List<PlayerCostReduction> CostReductions { get; } = new();

    /// <summary>
    /// guest-of-honor ("opponent cannot play events"), doomed-shugenja ("cannot place fate
    /// when playing [a copy of] this character"), grasp-of-earth ("opponent cannot play
    /// characters from hand"). consideredCard is the card being considered for whatever
    /// `action` describes (e.g. the card someone is trying to play) - needed to check a
    /// restriction's Qualifier (ringteki Restriction.js's "events"/"characters"/"source",
    /// the only three any ported card uses). A qualified restriction with no consideredCard
    /// to check against is "not restricted from this specific one", not an error - there's
    /// simply nothing to compare.
    /// </summary>
    public bool IsPlayerRestrictedFrom(Player player, string action, Card? consideredCard = null)
    {
        bool MatchesQualifiedRestriction(string action2, string? qualifier, Card source) =>
            action2 == action && MatchesPlayerQualifier(qualifier, source, consideredCard);

        if (PlayerRestrictions.Any(r => r.Target == player && MatchesQualifiedRestriction(r.Action, r.Qualifier, r.Source)))
            return true;

        return ActivePersistentEffectsAffectingPlayer(player).Any(pair =>
        {
            var isRestriction = EffectVocabulary.TryGetPlayerRestrictionAction(
                pair.Effect.GetProperty("name").GetString(),
                pair.Effect.TryGetProperty("value", out var v) ? v : (JsonElement?)null,
                out var restrictedAction,
                out var qualifier);
            return isRestriction && MatchesQualifiedRestriction(restrictedAction, qualifier, pair.Source);
        });
    }

    private static bool MatchesPlayerQualifier(string? qualifier, Card restrictionSource, Card? consideredCard)
    {
        if (qualifier is null)
            return true;

        if (consideredCard is null)
            return false;

        return qualifier switch
        {
            "events" => consideredCard.Type == CardType.Event,
            "characters" => consideredCard.Type == CardType.Character,
            "source" => consideredCard == restrictionSource,
            _ => throw new NotSupportedException($"Unknown playerCannot 'restricts' qualifier '{qualifier}'.")
        };
    }

    /// <summary>city-of-lies' reduceNextPlayedCardCost, floored at 0. No "consume after one use" semantics - see PlayerCostReduction's own doc comment.</summary>
    public int EffectiveCost(Card card, Player player)
    {
        var reduction = CostReductions
            .Where(r => r.Player == player && (r.AppliesTo is null || PredicateEvaluator.Evaluate(r.AppliesTo.Value, card, SourceContextFor(card))))
            .Sum(r => r.Amount);

        return Math.Max(0, (card.PrintedCost ?? 0) - reduction);
    }

    /// <summary>
    /// cloud-the-mind's whileAttached "blank" - a character with this attached has no text
    /// at all. Checked by AbilityExecutor.Prepare/PrepareTriggered against context.Source,
    /// so a blanked card's own actions/triggered abilities simply can't run. Only gates
    /// *running* its own abilities - a blanked card's persistentEffects/whileAttached grants
    /// to other cards aren't separately suppressed (no ported card's executable slice needs
    /// that half yet).
    /// </summary>
    public bool IsBlanked(Card card) =>
        ActiveWhileAttachedEffectsFor(card).Any(pair => pair.Effect.GetProperty("name").GetString() == "blank");

    /// <summary>
    /// ringteki Ring.getElements(): the current conflict's type/elements, plus whatever
    /// "addElementAsAttacker" persistentEffects the attacking characters (seeker-of-
    /// knowledge) contribute - checked here rather than inline in PredicateEvaluator's
    /// isDuringConflict so it can reuse HasAddEffect the same way HasKeyword/
    /// HasEffectiveTrait do. False (not an error) with no active conflict, matching the
    /// no-conflict-is-false convention used throughout.
    /// </summary>
    public bool ConflictHasType(string type) =>
        CurrentConflict is { } conflict
        && (type == conflict.ConflictType
            || conflict.Elements.Contains(type)
            || conflict.Attackers.Any(attacker => HasAddEffect(attacker, "addElementAsAttacker", type)));

    /// <summary>
    /// mirumoto-prodigy's "while attacking alone, the defending player may declare at most 1
    /// defender" - ringteki models this as a conflict-scoped static effect
    /// (EffectNames.RestrictNumberOfDefenders), but card-schema.json expresses it as an
    /// ordinary "match": "self" persistentEffect on the attacker, so it's queried the same
    /// way as addElementAsAttacker: scan the attacking card's own persistentEffects rather
    /// than a separate conflict-level field. No defender-declaration flow exists yet to
    /// enforce this automatically (matches this engine's general lack of a
    /// declare-defenders step) - int.MaxValue means "no restriction found".
    /// </summary>
    public int MaxDefendersFor(Card attacker) =>
        ActivePersistentEffectsAffecting(attacker)
            .Where(pair => pair.Effect.GetProperty("name").GetString() == "restrictNumberOfDefenders")
            .Select(pair => pair.Effect.GetProperty("value").GetInt32())
            .DefaultIfEmpty(int.MaxValue)
            .Min();

    /// <summary>
    /// way-of-the-dragon's "attachmentLimit": 1 ("only one copy of this attachment per
    /// character") - ringteki's checkForIllegalAttachments matches copies by card.name
    /// (this engine's Card.Id, shared across separate instances of the same printed card)
    /// and runs as a continuous "discard the excess" cleanup pass; no equivalent
    /// always-running effect loop exists here, so this is a pure query a test asserts
    /// directly (same convention as MaxDefendersFor) rather than an automatic discard.
    /// </summary>
    public bool ExceedsAttachmentLimit(Card attachment)
    {
        var limit = ActivePersistentEffectsAffecting(attachment)
            .Where(pair => pair.Effect.GetProperty("name").GetString() == "attachmentLimit")
            .Select(pair => (int?)pair.Effect.GetProperty("value").GetInt32())
            .FirstOrDefault();

        if (limit is not { } value || attachment.AttachedTo is not { } parent)
            return false;

        var sameCardCount = AllCards().Count(c => c.AttachedTo == parent && c.Id == attachment.Id);
        return sameCardCount > value;
    }

    /// <summary>
    /// way-of-the-dragon/court-mask/favored-mount/daimyo-s-favor's "attachmentMyControlOnly" -
    /// ringteki basecard.ts.canAttach: legal only if the acting player controls the target
    /// character, or the attachment's own controller does. Wired into
    /// PlayCardGameActionHandler, the one real attach execution path in this engine - checked
    /// *before* the card moves into play area, so (unlike ExceedsAttachmentLimit) this reads
    /// attachment.PersistentEffects directly rather than going through
    /// ActivePersistentEffectsAffecting's SourceLocation gate (most ported cards default that
    /// to "play area", which the card doesn't sit in yet at the moment this decides whether
    /// the attach is even legal - the restriction is a property of the printed card, not
    /// something that only turns on once it's already settled into a zone).
    /// </summary>
    public bool IsAttachRestricted(Card attachment, Card parent, Player actingPlayer)
    {
        var hasRestriction = attachment.PersistentEffects.Any(definition =>
            definition.Match is { ValueKind: JsonValueKind.String } match && match.GetString() == "self"
            && definition.Effects.Any(effect => effect.GetProperty("name").GetString() == "attachmentMyControlOnly"));

        return hasRestriction && actingPlayer != parent.Controller && attachment.Controller != parent.Controller;
    }

    /// <summary>favored-mount's "cavalry" while attached - see PredicateEvaluator.HasTrait. Checks both scans (like IsRestrictedFrom) since a grant can come from either a persistentEffect or a whileAttached effect.</summary>
    public bool HasEffectiveTrait(Card card, string trait) => HasAddEffect(card, "addTrait", trait);

    /// <summary>
    /// asahina-storyteller/magnificent-kimono/tattooed-wanderer's keywords (sincerity/pride/
    /// covert). No predicate op consumes keywords yet (nothing in the executable set needs
    /// to ask "does this card have keyword X"), so this is queried directly rather than
    /// through hasTrait's wiring - still real engine behavior, not a no-op.
    /// </summary>
    public bool HasKeyword(Card card, string keyword) =>
        HasAddEffect(card, "addKeyword", keyword)
        || LastingKeywordGrants.Any(g => g.Target == card && g.Keyword == keyword
            && (g.Condition is not { } condition || PredicateEvaluator.Evaluate(condition, card, SourceContextFor(card))));

    private bool HasAddEffect(Card card, string effectName, string value)
    {
        bool Matches((Card Source, JsonElement Effect) pair) =>
            pair.Effect.GetProperty("name").GetString() == effectName && pair.Effect.GetProperty("value").GetString() == value;

        return ActivePersistentEffectsAffecting(card).Any(Matches) || ActiveWhileAttachedEffectsFor(card).Any(Matches);
    }

    /// <summary>
    /// Scans every attachment currently attached to `parent` for applicable whileAttached
    /// effects - the attachment-scoped counterpart to ActivePersistentEffectsAffecting.
    /// Always applies to the parent specifically (no separate scope to choose, unlike
    /// PersistentEffectDefinition's Match/TargetController).
    /// </summary>
    private IEnumerable<(Card Source, JsonElement Effect)> ActiveWhileAttachedEffectsFor(Card parent)
    {
        foreach (var attachment in AllCards())
        {
            if (attachment.AttachedTo != parent)
                continue;

            var attachmentContext = SourceContextFor(attachment);

            foreach (var definition in attachment.WhileAttachedEffects)
            {
                if (definition.Condition is { } condition && !PredicateEvaluator.Evaluate(condition, attachment, attachmentContext))
                    continue;

                if (definition.Match is { } match && !PredicateEvaluator.Evaluate(match, parent, attachmentContext))
                    continue;

                foreach (var effect in definition.Effects)
                    yield return (attachment, effect);
            }
        }
    }

    /// <summary>
    /// Scans every persistentEffects[] entry on every in-play card and yields (source, effect)
    /// pairs for the ones currently applicable to `candidate` - the on-demand equivalent of
    /// LastingEffects/Restrictions for effects that never expire. See
    /// PersistentEffectDefinition's own doc comment for why this is a live scan rather than a
    /// materialized list.
    /// </summary>
    private IEnumerable<(Card Source, JsonElement Effect)> ActivePersistentEffectsAffecting(Card candidate)
    {
        foreach (var source in AllCards())
        {
            foreach (var definition in source.PersistentEffects)
            {
                // A null Match is a player-scoped effect (guest-of-honor/doomed-shugenja) -
                // only ActivePersistentEffectsAffectingPlayer considers those.
                if (definition.Match is not { } match)
                    continue;

                if (definition.SourceLocation != "any" && source.Location != definition.SourceLocation)
                    continue;

                var sourceContext = SourceContextFor(source);

                if (definition.Condition is { } condition && !PredicateEvaluator.Evaluate(condition, source, sourceContext))
                    continue;

                if (!MatchesTargetController(definition.TargetController, source.Controller, candidate.Controller))
                    continue;

                var isMatch = match.ValueKind == JsonValueKind.String
                    ? candidate == source
                    : PredicateEvaluator.Evaluate(match, candidate, sourceContext);

                if (!isMatch)
                    continue;

                foreach (var effect in definition.Effects)
                    yield return (source, effect);
            }
        }
    }

    /// <summary>
    /// The player-scoped counterpart to ActivePersistentEffectsAffecting - only considers
    /// entries with a null Match (guest-of-honor/doomed-shugenja), resolving the target
    /// directly from TargetController rather than filtering candidate cards by it (there's
    /// no candidate card here; a player-scoped effect targets a player outright).
    /// </summary>
    private IEnumerable<(Card Source, JsonElement Effect)> ActivePersistentEffectsAffectingPlayer(Player player)
    {
        foreach (var source in AllCards())
        {
            foreach (var definition in source.PersistentEffects)
            {
                if (definition.Match is not null)
                    continue;

                if (definition.SourceLocation != "any" && source.Location != definition.SourceLocation)
                    continue;

                var sourceContext = SourceContextFor(source);

                if (definition.Condition is { } condition && !PredicateEvaluator.Evaluate(condition, source, sourceContext))
                    continue;

                var targetPlayer = definition.TargetController switch
                {
                    "self" => source.Controller,
                    "opponent" => Opponent(source.Controller),
                    _ => throw new NotSupportedException($"Unknown persistentEffect targetController '{definition.TargetController}'.")
                };

                if (targetPlayer != player)
                    continue;

                foreach (var effect in definition.Effects)
                    yield return (source, effect);
            }
        }
    }

    private AbilityContext SourceContextFor(Card source) => new() { Game = this, Player = source.Controller, Source = source };

    private bool MatchesTargetController(string targetController, Player sourceController, Player candidateController) => targetController switch
    {
        "self" => candidateController == sourceController,
        "opponent" => candidateController == Opponent(sourceController),
        "any" => true,
        _ => throw new NotSupportedException($"Unknown persistentEffect targetController '{targetController}'.")
    };

    /// <summary>
    /// ringteki game.js beginRound(): queues DynastyPhase, DrawPhase, ConflictPhase,
    /// FatePhase, then loops back into a new DynastyPhase - Regroup is a real Phases enum
    /// value in Constants.ts, but this ringteki version's round loop never actually queues
    /// a separate Regroup phase (FatePhase's own steps cover readying cards/returning rings
    /// instead), so it's unreachable here too. This method only moves CurrentPhase/
    /// RoundNumber forward and expires untilEndOfPhase lasting effects; no other side
    /// effects yet (no fate collection, no card flipping) - those are added only once a
    /// card actually needs them.
    /// </summary>
    public void AdvancePhase()
    {
        CurrentPhase = CurrentPhase switch
        {
            Phase.Dynasty => Phase.Draw,
            Phase.Draw => Phase.Conflict,
            Phase.Conflict => Phase.Fate,
            Phase.Fate => Phase.Dynasty,
            _ => throw new NotSupportedException($"AdvancePhase does not support starting from '{CurrentPhase}'.")
        };

        if (CurrentPhase == Phase.Dynasty)
            RoundNumber++;

        LastingEffects.Clear();
        LastingKeywordGrants.Clear();
        Restrictions.Clear();
        PlayerRestrictions.Clear();
        CostReductions.Clear();
        RevertControlChanges(_ => true);
    }

    /// <summary>
    /// Clears the current conflict and expires its "untilEndOfConflict" lasting effects and
    /// restrictions - "untilEndOfPhase" ones outlive it, since a Conflict phase can (once the
    /// engine models it) contain several conflicts declared one after another. No caller in
    /// this engine yet drives repeated conflicts within a single phase, so this is exercised
    /// directly by tests for now rather than by a phase-step loop.
    /// </summary>
    public void EndConflict()
    {
        CurrentConflict = null;
        LastingEffects.RemoveAll(e => e.Duration == "untilEndOfConflict");
        LastingKeywordGrants.RemoveAll(g => g.Duration == "untilEndOfConflict");
        Restrictions.RemoveAll(r => r.Duration == "untilEndOfConflict");
        PlayerRestrictions.RemoveAll(r => r.Duration == "untilEndOfConflict");
        CostReductions.RemoveAll(r => r.Duration == "untilEndOfConflict");
        RevertControlChanges(c => c.Duration == "untilEndOfConflict");
    }
}
