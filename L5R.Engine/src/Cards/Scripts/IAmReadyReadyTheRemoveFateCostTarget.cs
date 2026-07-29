using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// i-am-ready: remove 1 fate from a friendly bowed Unicorn character to ready that same
/// character. ringteki resolves ready() against the removeFate cost's own chosen target
/// (context.costs.removeFate) - valueRef.contextPath has no way to reference a cost's
/// chosen target, but a script has no such restriction: it reads context.CostTarget
/// directly (the same field every removeFate-costed card's cost handler already populates)
/// and reuses RemoveFateGameActionHandler/ReadyGameActionHandler against it.
/// </summary>
public sealed class IAmReadyReadyTheRemoveFateCostTarget : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var target = context.CostTarget
            ?? throw new InvalidOperationException("i-am-ready requires context.CostTarget to be set.");

        if (target.Controller != context.Player)
            throw new InvalidOperationException($"'{target.Id}' must be controlled by the caster.");

        if (target.Faction != "unicorn")
            throw new InvalidOperationException($"'{target.Id}' must be a Unicorn character.");

        if (!target.Bowed)
            throw new InvalidOperationException($"'{target.Id}' must be bowed.");

        if (target.Fate <= 0)
            throw new InvalidOperationException($"'{target.Id}' has no fate to remove.");

        context.Target = target;
        new RemoveFateGameActionHandler().Execute(context, null);
        new ReadyGameActionHandler().Execute(context, null);
    }
}
