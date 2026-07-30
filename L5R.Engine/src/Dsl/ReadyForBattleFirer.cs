using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// ready-for-battle: an event card played from hand in reaction to a character its
/// controller controls being bowed by something other than that same controller's own
/// ability. Fired here at the one place a card gets bowed as a JSON-driven gameAction
/// (BowGameActionHandler.Execute, also the shape GameActionRegistry registers as "bow") -
/// context.Player there is whoever's ability caused the bow, so "not the target's own
/// controller" is exactly the state fact this script's own BowCausedBySelf field needs.
///
/// The real card's own carve-out ("or a ring effect", even one the target's own controller
/// happens to resolve) isn't modeled - this engine has no distinct "ring" source type
/// (ring effects resolve via ResolveConflictRingGameActionHandler with context.Player set to
/// the conflict's winner, not something separately taggable as "a ring, not a player"), so a
/// ring effect that happened to bow the resolving player's own character would be
/// (incorrectly, but rarely - no ported ring effect actually does this) treated as
/// self-caused. Doesn't cover BowSelfCostHandler's own bow-as-a-cost either, but that's
/// inherently always self-caused anyway, so it would never have triggered this regardless.
/// </summary>
public static class ReadyForBattleFirer
{
    public static void FireIfLegal(GameState game, Player bowingPlayer, Card target)
    {
        if (bowingPlayer == target.Controller)
            return;

        var controller = target.Controller;
        var readyForBattle = controller.Hand.FirstOrDefault(c => c.Id == "ready-for-battle");
        if (readyForBattle is null)
            return;

        var cost = game.EffectiveCost(readyForBattle, controller);
        if (controller.Fate < cost)
            return;

        controller.Fate -= cost;

        var context = new AbilityContext { Game = game, Player = controller, Source = readyForBattle, Target = target, BowCausedBySelf = false };
        new ReadyForBattleReadyOnOpponentOrRingBow().Execute(context);

        ZoneMover.MoveTo(readyForBattle, controller.Discard, "discard");
    }
}
