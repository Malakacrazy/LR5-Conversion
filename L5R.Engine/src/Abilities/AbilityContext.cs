using L5R.Engine.State;

namespace L5R.Engine.Abilities;

/// <summary>ringteki AbilityContext.ts: the game/player/source triple every check runs against.</summary>
public sealed class AbilityContext
{
    public required GameState Game { get; init; }
    public required Player Player { get; init; }
    public required Card Source { get; init; }

    /// <summary>ringteki context.target - set once a target has been chosen, null before then.</summary>
    public Card? Target { get; set; }

    /// <summary>
    /// ringteki context.costs[gameAction.name] - the card chosen to satisfy a parameterized
    /// cost (e.g. sacrifice's cardType/cardCondition selector), null before it's chosen.
    /// Only one parameterized cost per action exists in cards ported so far, so a single
    /// slot is enough; would need to key by cost name if that stops being true.
    /// </summary>
    public Card? CostTarget { get; set; }

    /// <summary>
    /// ringteki selectRingPrompt.js - which ring element the player chose, for gameActions
    /// like selectRing that need one. No ring-selection UI exists, so (like ChosenTarget)
    /// the caller supplies the choice directly; null until then.
    /// </summary>
    public string? ChosenRingElement { get; set; }

    /// <summary>
    /// ringteki ModifyBidAction's Direction.Prompt - "increase" or "decrease", for
    /// modifyBid's "direction": "prompt". No choice-prompt UI exists, so (like
    /// ChosenRingElement) the caller supplies the choice directly; null until then.
    /// </summary>
    public string? ChosenBidDirection { get; set; }

    /// <summary>
    /// ringteki TriggeredAbilityContext.event - which PendingAbility a "cancel" gameAction
    /// (forged-edict, voice-of-honor) should cancel. No interrupt-window scheduler exists to
    /// wire this automatically, so (like ChosenRingElement) the caller sets it directly on
    /// the cancelling ability's own context before running it.
    /// </summary>
    public Dsl.PendingAbility? InterruptedAbility { get; set; }
}
