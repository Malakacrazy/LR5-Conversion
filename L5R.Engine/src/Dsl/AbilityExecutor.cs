using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// Executes one already-parsed ActionDefinition or TriggeredAbilityDefinition: pay costs,
/// then run its (and its target's) gameActions. Target selection itself is not modeled yet
/// - the caller passes the chosen target directly, matching how far the interpreter has
/// grown so far (task 9's first slice: prove one simple real card runs end to end, not
/// build a full prompt pipeline). An action-level condition's implicit candidate is
/// context.Source, matching the convention established throughout card-porting (e.g.
/// kaiu-shuichi/mirumoto-prodigy).
///
/// Execute/ExecuteTriggered are Resolve(Prepare(...)) - see Prepare's doc comment for why
/// the split exists (cancel gameActions).
/// </summary>
public sealed class AbilityExecutor
{
    private readonly CostRegistry _costs;
    private readonly GameActionRegistry _gameActions;

    public AbilityExecutor(CostRegistry costs, GameActionRegistry gameActions)
    {
        _costs = costs;
        _gameActions = gameActions;
    }

    public bool IsConditionMet(ActionDefinition action, AbilityContext context) =>
        action.Condition is null || PredicateEvaluator.Evaluate(action.Condition.Value, context.Source, context);

    public void Execute(ActionDefinition action, AbilityContext context, Card? chosenTarget = null, Card? chosenCostTarget = null, string? chosenChoice = null, IReadOnlyList<Card>? chosenTargets = null, IReadOnlyDictionary<string, Ring>? chosenRingTargets = null) =>
        Resolve(Prepare(action, context, chosenTarget, chosenCostTarget, chosenChoice, chosenTargets, chosenRingTargets));

    /// <summary>
    /// Runs a triggeredAbilities[] entry. No event bus exists to know an event actually
    /// happened, so the caller asserts it did by passing the event's subject card directly;
    /// the when-clause's predicate is evaluated against it exactly like a normal
    /// cardCondition would be against a target candidate.
    ///
    /// ringteki CardAbility.js: isTriggeredAbility() && !card.canTriggerAbilities(context)
    /// blocks resolution - basecard.ts's canTriggerAbilities checks
    /// checkRestrictions('triggerAbilities'), i.e. a "cardCannot: triggerAbilities"
    /// cardLastingEffect placed on this card by some other card (hiruma-ambusher,
    /// tranquility). Plain actions (CardAction.js) have no equivalent check, so Execute
    /// deliberately doesn't gate on this - only triggered reactions/interrupts do.
    /// </summary>
    public void ExecuteTriggered(TriggeredAbilityDefinition ability, AbilityContext context, Card eventCard, Card? chosenTarget = null, Card? chosenCostTarget = null, string? chosenChoice = null) =>
        Resolve(PrepareTriggered(ability, context, eventCard, chosenTarget, chosenCostTarget, chosenChoice));

    /// <summary>
    /// Checks the action's condition/phase and pays its costs, but stops short of running
    /// its effects - the ringteki Event/EventWindow split (Event.js's `cancelled` flag),
    /// scoped down so a "cancel" gameAction (forged-edict, voice-of-honor) fired between
    /// Prepare and Resolve can actually prevent this ability's effects from ever applying.
    /// Execute is just Resolve(Prepare(...)) - existing callers that never interleave a
    /// cancel see no behavior change.
    /// </summary>
    public PendingAbility Prepare(ActionDefinition action, AbilityContext context, Card? chosenTarget = null, Card? chosenCostTarget = null, string? chosenChoice = null, IReadOnlyList<Card>? chosenTargets = null, IReadOnlyDictionary<string, Ring>? chosenRingTargets = null)
    {
        if (context.Game.IsBlanked(context.Source))
            throw new InvalidOperationException($"'{context.Source.Id}' has no text (blanked) and cannot use this action.");

        if (!IsConditionMet(action, context))
            throw new InvalidOperationException($"Action '{action.Title}' condition is not currently met.");

        if (action.Phase is not null && Phases.Parse(action.Phase) != context.Game.CurrentPhase)
            throw new InvalidOperationException(
                $"Action '{action.Title}' can only be used during the {action.Phase} phase.");

        if (context.Source.Location != action.Location)
            throw new InvalidOperationException(
                $"Action '{action.Title}' can only be used while '{context.Source.Id}' is in {action.Location}.");

        return PayCostsAndPrepare(action.Title, action.Costs, action.Target, action.Targets, action.SelectDependsOnTargets, action.GameActions, context, chosenTarget, chosenCostTarget, chosenChoice, chosenTargets, chosenRingTargets);
    }

    /// <summary>Triggered-ability counterpart to Prepare - see its doc comment.</summary>
    public PendingAbility PrepareTriggered(TriggeredAbilityDefinition ability, AbilityContext context, Card eventCard, Card? chosenTarget = null, Card? chosenCostTarget = null, string? chosenChoice = null)
    {
        if (context.Game.IsBlanked(context.Source))
            throw new InvalidOperationException($"'{context.Source.Id}' has no text (blanked) and cannot use this triggered ability.");

        if (context.Game.IsRestrictedFrom(context.Source, "triggerAbilities"))
            throw new InvalidOperationException($"'{context.Source.Id}' cannot trigger abilities right now.");

        if (!PredicateEvaluator.Evaluate(ability.WhenCondition, eventCard, context))
            throw new InvalidOperationException($"Triggered ability '{ability.Title}' when-condition is not met for event card '{eventCard.Id}'.");

        return PayCostsAndPrepare(ability.Title, ability.Costs, ability.Target, null, null, ability.GameActions, context, chosenTarget, chosenCostTarget, chosenChoice, null, null);
    }

    private PendingAbility PayCostsAndPrepare(
        string title,
        IReadOnlyList<CostDefinition> costs,
        TargetDefinition? target,
        IReadOnlyDictionary<string, RingTargetEntry>? targets,
        SelectDependsOnTargetsDefinition? selectDependsOnTargets,
        IReadOnlyList<GameActionDefinition> gameActions,
        AbilityContext context,
        Card? chosenTarget,
        Card? chosenCostTarget,
        string? chosenChoice,
        IReadOnlyList<Card>? chosenTargets,
        IReadOnlyDictionary<string, Ring>? chosenRingTargets)
    {
        context.CostTarget = chosenCostTarget;

        foreach (var cost in costs)
        {
            var handler = _costs.Resolve(cost.Name);
            if (!handler.CanPay(context, cost.Params))
                throw new InvalidOperationException($"Cost '{cost.Name}' cannot currently be paid.");
        }

        foreach (var cost in costs)
            _costs.Resolve(cost.Name).Pay(context, cost.Params);

        return new PendingAbility
        {
            Title = title,
            Target = target,
            Targets = targets,
            SelectDependsOnTargets = selectDependsOnTargets,
            GameActions = gameActions,
            Context = context,
            ChosenTarget = chosenTarget,
            ChosenChoice = chosenChoice,
            ChosenTargets = chosenTargets,
            ChosenRingTargets = chosenRingTargets
        };
    }

    /// <summary>Runs a PendingAbility's effects - a no-op if it was cancelled in the meantime (see CancelGameActionHandler).</summary>
    public void Resolve(PendingAbility pending)
    {
        if (pending.Cancelled)
            return;

        var title = pending.Title;
        var target = pending.Target;
        var context = pending.Context;
        var chosenTarget = pending.ChosenTarget;
        var chosenChoice = pending.ChosenChoice;
        var chosenTargets = pending.ChosenTargets;

        // card-schema.json's "targets" (plural) - a menu of simultaneous, independently-
        // targeted ring slots (know-the-world), mutually exclusive with the singular Target
        // below. No ring-selection UI exists, so the caller supplies which Ring fills each
        // named slot; each slot's own RingCondition is checked before running its GameAction.
        if (pending.Targets is { } targets)
        {
            var chosenRings = pending.ChosenRingTargets
                ?? throw new InvalidOperationException($"Ability '{title}' requires chosen ring targets but none were supplied.");

            foreach (var (name, entry) in targets)
            {
                var ring = chosenRings.TryGetValue(name, out var r)
                    ? r
                    : throw new InvalidOperationException($"Ability '{title}' requires a ring target named '{name}'.");

                if (!RingPredicateEvaluator.Evaluate(entry.RingCondition, ring, context))
                    throw new InvalidOperationException($"The chosen '{name}' ring does not satisfy ability '{title}''s ring condition.");

                context.TargetRing = ring;
                _gameActions.Resolve(entry.GameAction.Name).Execute(context, entry.GameAction.Params);
            }

            return;
        }

        // for-shame's "targets" shape: one plain card-pick slot plus one "mode": "select"
        // slot depending on it. chosenTarget fills the dependency slot (validated only
        // against its own CardCondition, same scope as the maxStat branch below - trust the
        // caller for cardType/controller, same as everywhere else); chosenChoice picks which
        // named choice's gameAction to run, targeting the dependency card directly rather
        // than resolving the JSON's own (redundant, given dependsOn) contextPath.
        if (pending.SelectDependsOnTargets is { } selectTargets)
        {
            var dependencyCard = chosenTarget
                ?? throw new InvalidOperationException($"Ability '{title}' requires a chosen target for '{selectTargets.DependencyName}'.");

            if (selectTargets.DependencyTarget.CardCondition is { } dependencyCondition
                && !PredicateEvaluator.Evaluate(dependencyCondition, dependencyCard, context))
                throw new InvalidOperationException($"'{dependencyCard.Id}' does not satisfy ability '{title}''s '{selectTargets.DependencyName}' target condition.");

            var label = chosenChoice
                ?? throw new InvalidOperationException($"Ability '{title}' requires a choice but none was supplied.");
            var chosenGameAction = selectTargets.Choices.TryGetValue(label, out var selectedGameAction)
                ? selectedGameAction
                : throw new InvalidOperationException($"Ability '{title}' has no choice named '{label}'.");

            context.Target = dependencyCard;
            _gameActions.Resolve(chosenGameAction.Name).Execute(context, chosenGameAction.Params);
            return;
        }

        // ringteki CardGameAction.defaultTargets: a gameAction with no explicit target
        // defaults to context.source, e.g. adept-of-shadows' returnToHand.
        context.Target = context.Source;
        RunGameActions(pending.GameActions, context);

        if (target is not null)
        {
            // "mode": "select" - a menu of named alternatives (kuroi-mori's "switch the
            // ring or switch the type"), not a card to pick. No selection UI exists, so
            // the caller supplies the chosen label directly, same convention as chosenTarget.
            if (target.Choices is not null)
            {
                var label = chosenChoice
                    ?? throw new InvalidOperationException($"Ability '{title}' requires a choice but none was supplied.");
                var chosenGameAction = target.Choices.TryGetValue(label, out var ga)
                    ? ga
                    : throw new InvalidOperationException($"Ability '{title}' has no choice named '{label}'.");

                context.Target = context.Source;
                _gameActions.Resolve(chosenGameAction.Name).Execute(context, chosenGameAction.Params);
                return;
            }

            // "mode": "maxStat" - up to NumCards cards (0 = unlimited) totaling at most
            // StatBudget of CardStat. No solver exists to search for a legal combination,
            // so the caller supplies the chosen set directly and this only validates it.
            if (target.MaxStat is { } maxStat)
            {
                var chosen = chosenTargets
                    ?? throw new InvalidOperationException($"Ability '{title}' requires chosen targets but none were supplied.");

                if (maxStat.NumCards > 0 && chosen.Count > maxStat.NumCards)
                    throw new InvalidOperationException($"Ability '{title}' allows at most {maxStat.NumCards} targets, got {chosen.Count}.");

                foreach (var card in chosen)
                {
                    if (target.CardCondition is { } condition && !PredicateEvaluator.Evaluate(condition, card, context))
                        throw new InvalidOperationException($"'{card.Id}' does not satisfy ability '{title}''s target condition.");
                }

                var budget = ValueRefResolver.ResolveInt(maxStat.StatBudget, context);
                var total = chosen.Sum(card => PredicateEvaluator.ResolveCardStat(maxStat.CardStat, card, context));
                if (total > budget)
                    throw new InvalidOperationException($"Ability '{title}''s chosen targets total {total} {maxStat.CardStat}, exceeding the budget of {budget}.");

                foreach (var card in chosen)
                {
                    context.Target = card;
                    RunGameActions(target.GameActions, context);
                }

                return;
            }

            // "mode": "upTo" (staging-ground's "flip up to 2 dynasty cards") - like MaxStat
            // above but with only a count cap, no cardStat/statBudget at all.
            if (target.UpToNumCards is { } upToNumCards)
            {
                var chosen = chosenTargets
                    ?? throw new InvalidOperationException($"Ability '{title}' requires chosen targets but none were supplied.");

                if (chosen.Count > upToNumCards)
                    throw new InvalidOperationException($"Ability '{title}' allows at most {upToNumCards} targets, got {chosen.Count}.");

                foreach (var card in chosen)
                {
                    if (target.CardCondition is { } condition && !PredicateEvaluator.Evaluate(condition, card, context))
                        throw new InvalidOperationException($"'{card.Id}' does not satisfy ability '{title}''s target condition.");

                    context.Target = card;
                    RunGameActions(target.GameActions, context);
                }

                return;
            }

            context.Target = chosenTarget
                ?? throw new InvalidOperationException($"Ability '{title}' requires a target but none was supplied.");

            RunGameActions(target.GameActions, context);
        }
    }

    /// <summary>
    /// ringteki semantics for a gameAction array: run every entry that can currently
    /// affect the target, not just the first one - e.g. against-the-waves' [bow, ready]
    /// relies on exactly one of the two being legal at a time. CanAffect is checked for
    /// every entry against the *pre-execution* state before any of them run - otherwise
    /// bow flipping Bowed to true would make ready's CanAffect see a now-bowed card and
    /// fire right after it, undoing the bow.
    ///
    /// An entry with its own Target override (e.g. the-art-of-peace's "honor all
    /// defenders") targets a completely different card set than the ambient context.Target,
    /// so it's run independently against every resolved candidate rather than joining the
    /// shared CanAffect race the other entries run against each other. The ambient target is
    /// saved and restored around that loop (court-mask's [returnToHand, dishonor w/ its own
    /// "source.parent" Target] is the first ported card to mix both shapes in one array) -
    /// without this, the last per-entry candidate would leak into context.Target and get
    /// used as the shared group's target instead of context.Source.
    /// </summary>
    private void RunGameActions(IReadOnlyList<GameActionDefinition> gameActions, AbilityContext context)
    {
        var ambientTarget = context.Target;

        foreach (var gameAction in gameActions.Where(ga => ga.Target is not null))
        {
            var handler = _gameActions.Resolve(gameAction.Name);
            foreach (var candidate in TargetResolver.ResolveAllCardsMatching(gameAction.Target!.Value, context))
            {
                context.Target = candidate;
                handler.Execute(context, gameAction.Params);
            }
        }

        context.Target = ambientTarget;

        var sharedTarget = gameActions.Where(ga => ga.Target is null).ToList();
        var toRun = sharedTarget
            .Select(gameAction => (gameAction, handler: _gameActions.Resolve(gameAction.Name)))
            .Where(entry => sharedTarget.Count == 1 || entry.handler.CanAffect(context, entry.gameAction.Params))
            .ToList();

        foreach (var (gameAction, handler) in toRun)
            handler.Execute(context, gameAction.Params);
    }
}
