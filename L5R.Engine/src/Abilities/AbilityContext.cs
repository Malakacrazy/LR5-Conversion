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

    /// <summary>
    /// ringteki ChosenDiscardAction: which specific cards from the affected player's hand
    /// get discarded - a real player choice (via promptForSelect), not something this
    /// engine's chosenDiscard gameAction resolves itself. Same caller-supplies-the-choice
    /// convention as ChosenRingElement; null until set.
    /// </summary>
    public IReadOnlyList<Card>? ChosenDiscardCards { get; set; }

    /// <summary>
    /// ringteki RingAction's target - which ring a ring-scoped gameAction (returnRing,
    /// takeRing) affects. No ring-target selection/prompt pipeline exists, so (like
    /// ChosenRingElement) the caller sets this directly; null until then.
    /// </summary>
    public Ring? TargetRing { get; set; }

    /// <summary>
    /// ringteki costs.js payFateToRing: which unclaimed ring the player chose to place fate
    /// on - a parameterized cost's selection, like CostTarget but for a ring instead of a
    /// card. Kept separate from TargetRing so a future card needing both a cost-ring and a
    /// gameAction-ring in the same ability wouldn't collide.
    /// </summary>
    public Ring? CostRingTarget { get; set; }

    /// <summary>
    /// ringteki PlayAttachmentAction's target prompt: the character an attachment being
    /// played (playCard gameAction, guidance-of-the-ancestors) attaches to. No attach-target
    /// selection/legal-target search exists, so (like ChosenRingElement) the caller sets
    /// this directly; null until then.
    /// </summary>
    public Card? PlayAttachTarget { get; set; }

    /// <summary>
    /// ringteki DeckSearchAction: which card (among the top "amount" cards of the deck) the
    /// player took, or null for ringteki's own "Take nothing" - a legal choice, not a
    /// missing one. No search/selection-menu UI exists, so (like every other ChosenX field)
    /// the caller sets this directly; null until then.
    /// </summary>
    public Card? ChosenDeckSearchCard { get; set; }
}
