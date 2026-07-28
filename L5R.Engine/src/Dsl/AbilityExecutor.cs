using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// Executes one already-parsed ActionDefinition: pay costs, then run its (and its
/// target's) gameActions. Target selection itself is not modeled yet - the caller passes
/// the chosen target directly, matching how far the interpreter has grown so far (task
/// 9's first slice: prove one simple real card runs end to end, not build a full prompt
/// pipeline). condition is not evaluated yet - throws if present, per the same
/// fail-loud-not-silent policy as TargetResolver.
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

    public void Execute(ActionDefinition action, AbilityContext context, Card? chosenTarget = null)
    {
        if (action.Condition is not null)
            throw new NotSupportedException("AbilityExecutor does not yet evaluate action conditions.");

        foreach (var cost in action.Costs)
        {
            var handler = _costs.Resolve(cost.Name);
            if (!handler.CanPay(context, cost.Params))
                throw new InvalidOperationException($"Cost '{cost.Name}' cannot currently be paid.");
        }

        foreach (var cost in action.Costs)
            _costs.Resolve(cost.Name).Pay(context, cost.Params);

        foreach (var gameAction in action.GameActions)
            _gameActions.Resolve(gameAction.Name).Execute(context, gameAction.Params);

        if (action.Target is not null)
        {
            context.Target = chosenTarget
                ?? throw new InvalidOperationException($"Action '{action.Title}' requires a target but none was supplied.");

            foreach (var gameAction in action.Target.GameActions)
                _gameActions.Resolve(gameAction.Name).Execute(context, gameAction.Params);
        }
    }
}
