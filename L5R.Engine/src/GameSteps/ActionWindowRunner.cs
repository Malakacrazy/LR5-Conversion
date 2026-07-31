using System.Collections;
using System.Collections.Generic;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.Logging;
using L5R.Engine.Scheduling;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps;

/// <summary>
/// The alternating-priority action window shared by GameLoop's pre-conflict window
/// (ringteki's own "preConflict" ActionWindow, starting with the current first player) and
/// ConflictResolver's mid-conflict window (ringteki's ConflictActionWindow, starting with
/// the defender - real rule: the defender acts first once defenders are declared). Extracted
/// out of GameLoop (which originally only ever ran this once, before conflicts existed) so
/// ConflictResolver - a static, GameLoop-independent class by design, so it stays unit-
/// testable without any Scheduler/GameLoop involved (see ConflictResolverTests) - can run the
/// exact same loop mid-conflict without duplicating it.
///
/// Each turn: a currently-legal scripted (Phase B) action, then a plain CardAction
/// (abilities.actions[]), then a card played from hand, or pass - in that priority order (a
/// minor, arbitrary choice, not a real rule). Two consecutive passes ends it.
///
/// usedThisWindow guards against an infinite loop: several scripted actions (e.g.
/// fearsome-mystic) have no bow-self cost or other self-invalidating side effect, so
/// IsLegal/ChooseScriptedAction would keep saying "yes" for the same card forever and
/// consecutivePasses would never reach 2. Each physical Card can act at most once per window
/// (reference equality). Known limitation: a bot controlling two eligible copies of the same
/// scripted-action card only ever gets offered the first one, even once it's used up - not
/// worth a stateful policy redesign for a trivial bot and an edge case no fixed harness deck hits.
/// </summary>
public static class ActionWindowRunner
{
    /// <summary>
    /// An IEnumerator so a real (human-backed) policy can pause mid-window: each policy call
    /// is wrapped in a StepAwait, so Scheduler genuinely suspends here instead of the caller
    /// blocking a thread waiting for an instant answer. Callers forward this via `foreach (var
    /// _ in Run(...)) yield return _;` rather than treating it as an independent Scheduler
    /// frame - see GameLoop/ConflictResolver's own call sites.
    /// </summary>
    public static IEnumerator Run(GameState game, Player startingPlayer, IReadOnlyDictionary<Player, IBotPolicy> policies, EventLog? eventLog)
    {
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());
        var current = startingPlayer;
        var consecutivePasses = 0;
        var usedThisWindow = new HashSet<Card>();

        while (consecutivePasses < 2)
        {
            var scriptedTask = policies[current].ChooseScriptedAction(game, current);
            yield return new StepAwait(scriptedTask);
            var scripted = scriptedTask.Result;

            if (scripted is { } chosen && usedThisWindow.Add(chosen.Source))
            {
                consecutivePasses = 0;
                chosen.Action.Invoke(game, chosen.Source, current);
                eventLog?.Record("scriptedActionActivated", new Dictionary<string, string> { ["player"] = current.Name, ["card"] = chosen.Source.Id });
            }
            else
            {
                var actionTask = policies[current].ChooseAction(game, current);
                yield return new StepAwait(actionTask);
                var action = actionTask.Result;

                if (action is not null && usedThisWindow.Add(action.Card))
                {
                    consecutivePasses = 0;
                    var context = new AbilityContext { Game = game, Player = current, Source = action.Card };
                    executor.Execute(action.Definition!, context);
                    eventLog?.Record("actionActivated", new Dictionary<string, string> { ["player"] = current.Name, ["card"] = action.Card.Id, ["action"] = action.Title });
                }
                else
                {
                    var playTask = policies[current].ChoosePlay(game, current, "hand");
                    yield return new StepAwait(playTask);
                    var handCard = playTask.Result;

                    if (handCard is not null)
                    {
                        consecutivePasses = 0;
                        var context = new AbilityContext { Game = game, Player = current, Source = handCard };
                        new PlayCardGameActionHandler().Execute(context, null);
                        eventLog?.Record("cardPlayed", new Dictionary<string, string> { ["player"] = current.Name, ["card"] = handCard.Id, ["from"] = "hand" });

                        if (handCard.Type == CardType.Event)
                        {
                            var scriptTask = policies[current].ResolveEventScript(handCard.Id);
                            yield return new StepAwait(scriptTask);
                            EventResolver.ResolveAndDiscard(game, handCard, current, scriptTask.Result);
                            eventLog?.Record("eventResolved", new Dictionary<string, string> { ["player"] = current.Name, ["card"] = handCard.Id });
                        }
                    }
                    else
                    {
                        consecutivePasses++;
                    }
                }
            }

            current = game.Opponent(current);
        }
    }
}
