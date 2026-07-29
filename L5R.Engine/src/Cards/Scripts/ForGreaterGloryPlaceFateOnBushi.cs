using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// for-greater-glory: after breaking a province during a military conflict as the
/// attacking player, place 1 fate on each Bushi character you control in the conflict
/// (province-breaking isn't otherwise modeled, so the caller asserts it happened via
/// Conflict.ConflictType/AttackingPlayer, same convention as shinjo-altansarnai). No
/// throw for zero matching Bushi - a bulk target with nothing to affect is a legal no-op,
/// same as allCardsMatching's own empty-result handling elsewhere. "max: perConflict(1)"
/// needs no work, matching every "max"/"limit" field's established no-op precedent.
/// </summary>
public sealed class ForGreaterGloryPlaceFateOnBushi : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException("for-greater-glory requires an active conflict.");

        if (conflict.ConflictType != "military")
            throw new InvalidOperationException("for-greater-glory can only be used during a military conflict.");

        if (conflict.AttackingPlayer != context.Player)
            throw new InvalidOperationException("for-greater-glory can only be used while its controller is the attacking player.");

        var myBushiInConflict = conflict.Attackers.Concat(conflict.Defenders)
            .Where(c => c.Controller == context.Player && c.Traits.Contains("bushi"))
            .ToList();

        foreach (var card in myBushiInConflict)
        {
            context.Target = card;
            new PlaceFateGameActionHandler().Execute(context, null);
        }
    }
}
