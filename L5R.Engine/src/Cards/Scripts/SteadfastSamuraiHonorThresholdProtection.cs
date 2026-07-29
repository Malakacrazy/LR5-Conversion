using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// steadfast-samurai: after the fate phase begins, if its controller's honor is at least 5
/// more than the opponent's, this character can't be discarded or have fate removed until
/// the end of the phase. Adds two CardRestriction entries directly rather than routing
/// through CardLastingEffectGameActionHandler's generic "cardCannot" (which already
/// supports arbitrary action strings) - the gap here is the forcedReaction's own trigger
/// condition (phase start + honor comparison), not the restriction mechanism itself.
/// RemoveFateGameActionHandler/DiscardFromPlayGameActionHandler previously never checked
/// IsRestrictedFrom at all (no ported card needed to block either action before now) -
/// both now do, so this restriction actually takes effect.
/// </summary>
public sealed class SteadfastSamuraiHonorThresholdProtection : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var samurai = context.Source;

        if (context.Game.CurrentPhase != Phase.Fate)
            throw new InvalidOperationException($"'{samurai.Id}' can only trigger at the start of the fate phase.");

        var opponent = context.Game.Opponent(context.Player);
        if (context.Player.Honor < opponent.Honor + 5)
            throw new InvalidOperationException($"'{samurai.Id}' requires its controller's honor to be at least 5 more than the opponent's.");

        context.Game.Restrictions.Add(new CardRestriction { Target = samurai, Action = "removeFate", Duration = "untilEndOfPhase" });
        context.Game.Restrictions.Add(new CardRestriction { Target = samurai, Action = "discardFromPlay", Duration = "untilEndOfPhase" });
    }
}
