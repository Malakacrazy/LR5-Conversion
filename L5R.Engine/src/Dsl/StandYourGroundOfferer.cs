using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// stand-your-ground: a "wouldInterrupt" event card played from hand in response to an
/// honored character its controller controls being discarded from play - instead of the
/// character actually leaving play, only its honored status is discarded. Same shape as
/// forged-edict/voice-of-honor (see WouldInterruptOfferer's own doc comment), but the thing
/// it interrupts is a leaves-play mutation rather than another event's effect, so it's
/// offered at the top of DiscardFromPlayGameActionHandler.Execute instead - the canonical
/// shared "discard a card from play" primitive, same "hook the shared primitive, not every
/// call site" scope already accepted for vengeful-berserker (a sacrifice cost's own
/// ZoneMover.MoveTo call bypasses this and isn't covered).
///
/// "Always play when legal" - same trivial-bot heuristic as every other adopted card. Unlike
/// forged-edict/voice-of-honor this is unconditionally beneficial to play whenever legal (it
/// saves the character from actually leaving play), so there's no scenario where holding it
/// back would be better for a trivial bot.
/// </summary>
public static class StandYourGroundOfferer
{
    public static bool TryInterrupt(GameState game, Card target)
    {
        if (!target.IsHonored)
            return false;

        var controller = target.Controller;
        var standYourGround = controller.Hand.FirstOrDefault(c => c.Id == "stand-your-ground");
        if (standYourGround is null)
            return false;

        var cost = game.EffectiveCost(standYourGround, controller);
        if (controller.Fate < cost)
            return false;

        controller.Fate -= cost;

        var context = new AbilityContext { Game = game, Player = controller, Source = standYourGround, Target = target };
        new StandYourGroundDiscardTokenInsteadOfLeavingPlay().Execute(context);

        ZoneMover.MoveTo(standYourGround, controller.Discard, "discard");
        return true;
    }
}
