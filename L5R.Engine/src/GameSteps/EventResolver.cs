using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps;

/// <summary>
/// Ports ringteki's Event.play()/executeHandler(): once an event card is played (paying its
/// own printed cost via PlayCardGameActionHandler, which is left completely unchanged - see
/// below for why), it briefly enters play, its own single ability resolves immediately (a
/// plain abilities.actions[] entry - card-schema.json doesn't model events any differently
/// from a character's activated action; an event's whole reason to exist is just this one
/// resolution, not something to hold for later repeated use), then the card moves to its
/// controller's discard pile - it never lingers in PlayArea.
///
/// This was a real, previously undiscovered gap: PlayCardGameActionHandler moves *every* card
/// type into PlayArea uniformly and stops there (correct and unchanged for characters/
/// holdings/attachments), so nothing ever resolved an event's own action or discarded it
/// afterward - GameLoop.RunPlayWindow calls this immediately after playing an event to close
/// that gap, rather than teaching PlayCardGameActionHandler itself about event-specific
/// follow-up (BlackmailTests/GoodOmenTests already call PlayCardGameActionHandler directly to
/// test just its CanPlay gate, asserting the card stays in PlayArea afterward - correct for
/// what those tests exercise, since neither goes through GameLoop at all).
///
/// Target selection covers only the shapes a trivial bot needs: no target, or a plain
/// single-Card TargetDefinition (the first legal candidate, matching
/// FirstLegalActionBotPolicy's own "first legal option" philosophy). "select"/"maxStat"/
/// ring-target shapes are out of scope for v1 - the event still gets discarded (it was still
/// played), but its effect simply doesn't fire if it needs one of those richer shapes.
/// </summary>
public static class EventResolver
{
    public static void ResolveAndDiscard(GameState game, Card eventCard, Player player)
    {
        if (eventCard.Actions.FirstOrDefault() is { Definition: { } definition } action)
        {
            var context = new AbilityContext { Game = game, Player = player, Source = eventCard };
            if (action.MeetsRequirements(context))
            {
                var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());
                switch (definition.Target)
                {
                    case null:
                        executor.Execute(definition, context);
                        break;
                    case { Choices: null, MaxStat: null } target:
                        var chosenTarget = TargetResolver.ResolveLegalTargets(target, context).FirstOrDefault();
                        executor.Execute(definition, context, chosenTarget: chosenTarget);
                        break;
                    // "select"/"maxStat" target shapes: not handled by a trivial bot yet - the
                    // event is still discarded below, its effect just doesn't fire.
                }
            }
        }

        ZoneMover.MoveTo(eventCard, player.Discard, "discard");
    }
}
