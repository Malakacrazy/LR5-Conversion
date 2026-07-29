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

    public void Execute(ActionDefinition action, AbilityContext context, Card? chosenTarget = null, Card? chosenCostTarget = null)
    {
        if (!IsConditionMet(action, context))
            throw new InvalidOperationException($"Action '{action.Title}' condition is not currently met.");

        if (action.Phase is not null && Phases.Parse(action.Phase) != context.Game.CurrentPhase)
            throw new InvalidOperationException(
                $"Action '{action.Title}' can only be used during the {action.Phase} phase.");

        RunCostsTargetAndGameActions(action.Title, action.Costs, action.Target, action.GameActions, context, chosenTarget, chosenCostTarget);
    }

    /// <summary>
    /// Runs a triggeredAbilities[] entry. No event bus exists to know an event actually
    /// happened, so the caller asserts it did by passing the event's subject card directly;
    /// the when-clause's predicate is evaluated against it exactly like a normal
    /// cardCondition would be against a target candidate.
    /// </summary>
    public void ExecuteTriggered(TriggeredAbilityDefinition ability, AbilityContext context, Card eventCard, Card? chosenTarget = null, Card? chosenCostTarget = null)
    {
        if (!PredicateEvaluator.Evaluate(ability.WhenCondition, eventCard, context))
            throw new InvalidOperationException($"Triggered ability '{ability.Title}' when-condition is not met for event card '{eventCard.Id}'.");

        RunCostsTargetAndGameActions(ability.Title, ability.Costs, ability.Target, ability.GameActions, context, chosenTarget, chosenCostTarget);
    }

    private void RunCostsTargetAndGameActions(
        string title,
        IReadOnlyList<CostDefinition> costs,
        TargetDefinition? target,
        IReadOnlyList<GameActionDefinition> gameActions,
        AbilityContext context,
        Card? chosenTarget,
        Card? chosenCostTarget)
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

        // ringteki CardGameAction.defaultTargets: a gameAction with no explicit target
        // defaults to context.source, e.g. adept-of-shadows' returnToHand.
        context.Target = context.Source;
        RunGameActions(gameActions, context);

        if (target is not null)
        {
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
    /// shared CanAffect race the other entries run against each other.
    /// </summary>
    private void RunGameActions(IReadOnlyList<GameActionDefinition> gameActions, AbilityContext context)
    {
        foreach (var gameAction in gameActions.Where(ga => ga.Target is not null))
        {
            var handler = _gameActions.Resolve(gameAction.Name);
            foreach (var candidate in TargetResolver.ResolveAllCardsMatching(gameAction.Target!.Value, context))
            {
                context.Target = candidate;
                handler.Execute(context, gameAction.Params);
            }
        }

        var sharedTarget = gameActions.Where(ga => ga.Target is null).ToList();
        var toRun = sharedTarget
            .Select(gameAction => (gameAction, handler: _gameActions.Resolve(gameAction.Name)))
            .Where(entry => sharedTarget.Count == 1 || entry.handler.CanAffect(context, entry.gameAction.Params))
            .ToList();

        foreach (var (gameAction, handler) in toRun)
            handler.Execute(context, gameAction.Params);
    }
}
