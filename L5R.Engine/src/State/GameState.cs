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

    public Player Opponent(Player player) => player == Player1 ? Player2 : Player1;

    /// <summary>Null outside of a conflict - see Conflict's own doc comment for what's deliberately not modeled yet.</summary>
    public Conflict? CurrentConflict { get; set; }

    /// <summary>
    /// Active cardLastingEffect modifiers - see CardLastingEffectGameActionHandler and
    /// LastingEffect's own doc comment for the two durations stored here and how each expires.
    /// </summary>
    public List<LastingEffect> LastingEffects { get; } = new();

    public int EffectiveGlory(Card card) => EffectiveStat(card, "glory", card.PrintedGlory);

    public int EffectiveMilitarySkill(Card card) => EffectiveStat(card, "military", card.PrintedMilitarySkill);

    public int EffectivePoliticalSkill(Card card) => EffectiveStat(card, "political", card.PrintedPoliticalSkill);

    public int EffectiveProvinceStrength(Card card) => EffectiveStat(card, "provinceStrength", card.PrintedProvinceStrength);

    private int EffectiveStat(Card card, string stat, int? printedValue)
    {
        var total = (printedValue ?? 0) + LastingEffects.Where(e => e.Target == card && e.Stat == stat).Sum(e => e.Value);

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

    public bool IsRestrictedFrom(Card card, string action)
    {
        if (Restrictions.Any(r => r.Target == card && r.Action == action && MatchesQualifier(r.Qualifier)))
            return true;

        bool MatchesAction((Card Source, JsonElement Effect) pair)
        {
            var isRestriction = EffectVocabulary.TryGetRestrictionAction(
                pair.Effect.GetProperty("name").GetString(),
                pair.Effect.TryGetProperty("value", out var v) ? v : (JsonElement?)null,
                out var restrictedAction,
                out var qualifier);
            return isRestriction && restrictedAction == action && MatchesQualifier(qualifier);
        }

        return ActivePersistentEffectsAffecting(card).Any(MatchesAction)
            || ActiveWhileAttachedEffectsFor(card).Any(MatchesAction);
    }

    /// <summary>A restriction with no qualifier always applies; one with a qualifier (pacifism's "military") only applies during a conflict of that type/element - same check as isDuringConflict's "type" filter.</summary>
    private bool MatchesQualifier(string? qualifier) =>
        qualifier is null || (CurrentConflict is { } conflict && (qualifier == conflict.ConflictType || conflict.Elements.Contains(qualifier)));

    /// <summary>favored-mount's "cavalry" while attached - see PredicateEvaluator.HasTrait. Checks both scans (like IsRestrictedFrom) since a grant can come from either a persistentEffect or a whileAttached effect.</summary>
    public bool HasEffectiveTrait(Card card, string trait) => HasAddEffect(card, "addTrait", trait);

    /// <summary>
    /// asahina-storyteller/magnificent-kimono/tattooed-wanderer's keywords (sincerity/pride/
    /// covert). No predicate op consumes keywords yet (nothing in the executable set needs
    /// to ask "does this card have keyword X"), so this is queried directly rather than
    /// through hasTrait's wiring - still real engine behavior, not a no-op.
    /// </summary>
    public bool HasKeyword(Card card, string keyword) => HasAddEffect(card, "addKeyword", keyword);

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
                if (definition.SourceLocation != "any" && source.Location != definition.SourceLocation)
                    continue;

                var sourceContext = SourceContextFor(source);

                if (definition.Condition is { } condition && !PredicateEvaluator.Evaluate(condition, source, sourceContext))
                    continue;

                if (!MatchesTargetController(definition.TargetController, source.Controller, candidate.Controller))
                    continue;

                var isMatch = definition.Match.ValueKind == JsonValueKind.String
                    ? candidate == source
                    : PredicateEvaluator.Evaluate(definition.Match, candidate, sourceContext);

                if (!isMatch)
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
        Restrictions.Clear();
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
        Restrictions.RemoveAll(r => r.Duration == "untilEndOfConflict");
        RevertControlChanges(c => c.Duration == "untilEndOfConflict");
    }
}
