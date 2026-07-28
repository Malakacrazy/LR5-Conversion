using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// Executes one already-parsed ActionDefinition: pay costs, then run its (and its
/// target's) gameActions. Target selection itself is not modeled yet - the caller passes
/// the chosen target directly, matching how far the interpreter has grown so far (task
/// 9's first slice: prove one simple real card runs end to end, not build a full prompt
/// pipeline). An action-level condition's implicit candidate is context.Source, matching
/// the convention established throughout card-porting (e.g. kaiu-shuichi/mirumoto-prodigy).
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

    public void Execute(ActionDefinition action, AbilityContext context, Card? chosenTarget = null)
    {
        if (!IsConditionMet(action, context))
            throw new InvalidOperationException($"Action '{action.Title}' condition is not currently met.");

        foreach (var cost in action.Costs)
        {
            var handler = _costs.Resolve(cost.Name);
            if (!handler.CanPay(context, cost.Params))
                throw new InvalidOperationException($"Cost '{cost.Name}' cannot currently be paid.");
        }

        foreach (var cost in action.Costs)
            _costs.Resolve(cost.Name).Pay(context, cost.Params);

        // ringteki CardGameAction.defaultTargets: a gameAction with no explicit target
        // defaults to context.source, e.g. adept-of-shadows' returnToHand.
        context.Target = context.Source;
        RunGameActions(action.GameActions, context);

        if (action.Target is not null)
        {
            context.Target = chosenTarget
                ?? throw new InvalidOperationException($"Action '{action.Title}' requires a target but none was supplied.");

            RunGameActions(action.Target.GameActions, context);
        }
    }

    /// <summary>
    /// ringteki semantics for a gameAction array: run every entry that can currently
    /// affect the target, not just the first one - e.g. against-the-waves' [bow, ready]
    /// relies on exactly one of the two being legal at a time. CanAffect is checked for
    /// every entry against the *pre-execution* state before any of them run - otherwise
    /// bow flipping Bowed to true would make ready's CanAffect see a now-bowed card and
    /// fire right after it, undoing the bow.
    /// </summary>
    private void RunGameActions(IReadOnlyList<GameActionDefinition> gameActions, AbilityContext context)
    {
        var toRun = gameActions
            .Select(gameAction => (gameAction, handler: _gameActions.Resolve(gameAction.Name)))
            .Where(entry => gameActions.Count == 1 || entry.handler.CanAffect(context, entry.gameAction.Params))
            .ToList();

        foreach (var (gameAction, handler) in toRun)
            handler.Execute(context, gameAction.Params);
    }
}
