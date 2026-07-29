using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// obstinate-recruit: discard this character once the opponent is more honorable than its
/// controller. ringteki models this as a delayedEffect (a self-monitoring nested condition+
/// gameAction) inside a persistentEffect - a shape the schema's flat persistentEffect
/// fields can't hold (matches this card's own scriptOverride reason). No standing listener
/// re-checks this automatically; like every other reactive script this session, the caller
/// invokes Execute whenever the condition might now hold (e.g. right after an honor
/// change), and it discards if the condition currently holds, throwing otherwise.
/// </summary>
public sealed class ObstinateRecruitDiscardWhenOpponentMoreHonorable : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var recruit = context.Source;
        var opponent = context.Game.Opponent(context.Player);

        if (context.Player.Honor >= opponent.Honor)
            throw new InvalidOperationException($"'{recruit.Id}' can only trigger when the opponent is more honorable than its controller.");

        context.Target = recruit;
        new DiscardFromPlayGameActionHandler().Execute(context, null);
    }
}
