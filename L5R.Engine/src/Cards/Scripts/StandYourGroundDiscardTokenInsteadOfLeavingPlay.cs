using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// stand-your-ground: when an honored character its controller controls would leave play,
/// discard that character's honored status token instead. Same "no new mechanism needed"
/// reasoning as reprieve - the caller calls this script instead of whatever would have
/// removed the character. This engine flattens ringteki's personalHonor status-token
/// object onto Card.IsHonored/IsDishonored directly, so DiscardStatusTokenGameActionHandler
/// (already built for that flattening) is reused completely untouched.
///
/// CanPlay is unconditionally false - same reasoning as ready-for-battle's own doc comment:
/// without it, the generic hand-play window would happily discard this with no effect at
/// any time, before StandYourGroundOfferer's own DiscardFromPlayGameActionHandler hook (the
/// card's real, only legitimate trigger, which bypasses this restriction entirely) ever gets
/// a chance.
/// </summary>
public sealed class StandYourGroundDiscardTokenInsteadOfLeavingPlay : ICardScript
{
    public bool CanPlay(AbilityContext context) => false;

    public void Execute(AbilityContext context)
    {
        var standYourGround = context.Source;

        var target = context.Target
            ?? throw new InvalidOperationException($"'{standYourGround.Id}' requires context.Target (the character that would leave play) to be set.");

        if (target.Controller != context.Player)
            throw new InvalidOperationException($"'{target.Id}' must be controlled by '{standYourGround.Id}''s controller.");

        if (!target.IsHonored)
            throw new InvalidOperationException($"'{target.Id}' must be honored.");

        new DiscardStatusTokenGameActionHandler().Execute(context, null);
    }
}
