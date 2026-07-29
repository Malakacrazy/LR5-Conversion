using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// solemn-scholar: even from home (no isParticipating check on the source, matching
/// ringteki exactly), if the earth ring is claimed by this character's controller, bow an
/// attacking character. "Considered claimed" is just Ring.ClaimedBy == the player here - a
/// specific-ring-ownership check beyond countClaimedRings' aggregate count.
/// </summary>
public sealed class SolemnScholarBowAttackerIfEarthClaimed : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var scholar = context.Source;
        var earthRing = context.Game.Rings.Single(r => r.Element == "earth");

        if (earthRing.ClaimedBy != context.Player)
            throw new InvalidOperationException($"'{scholar.Id}' can only be used while the earth ring is claimed by its controller.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{scholar.Id}' requires context.Target to be set.");

        if (context.Game.CurrentConflict?.Attackers.Contains(target) != true)
            throw new InvalidOperationException($"'{target.Id}' is not attacking.");

        new BowGameActionHandler().Execute(context, null);
    }
}
