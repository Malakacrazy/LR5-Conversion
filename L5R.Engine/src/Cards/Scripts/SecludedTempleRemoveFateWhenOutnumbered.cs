using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// secluded-temple: after the conflict phase begins, if this holding's controller has
/// fewer characters in play than their opponent, remove 1 fate from an opponent's
/// character. Needs GameState.CurrentPhase field inspection and a per-player card-count
/// comparison, neither modeled by the closed predicate vocabulary (matches this card's
/// own scriptOverride reason).
/// </summary>
public sealed class SecludedTempleRemoveFateWhenOutnumbered : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var temple = context.Source;

        if (context.Game.CurrentPhase != Phase.Conflict)
            throw new InvalidOperationException($"'{temple.Id}' can only trigger at the start of the conflict phase.");

        var opponent = context.Game.Opponent(context.Player);

        if (context.Player.PlayArea.Count >= opponent.PlayArea.Count)
            throw new InvalidOperationException($"'{temple.Id}' can only trigger when its controller has fewer characters in play than the opponent.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{temple.Id}' requires context.Target to be set.");

        if (target.Controller != opponent)
            throw new InvalidOperationException($"'{target.Id}' must be controlled by the opponent.");

        new RemoveFateGameActionHandler().Execute(context, null);
    }
}
