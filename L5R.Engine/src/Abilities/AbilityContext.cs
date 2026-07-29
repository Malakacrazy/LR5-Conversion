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

    /// <summary>
    /// ringteki CardMenuAction: which card the acting player picked from the menu (kitsuki-
    /// investigator's "player.opponent.hand"). No selection-menu UI exists, so (like every
    /// other ChosenX field) the caller supplies this directly; null until then.
    /// </summary>
    public Card? ChosenCardMenuCard { get; set; }

    /// <summary>
    /// ringteki ChooseAction / "mode": "select" target's own chosen label (city-of-the-open-
    /// hand's "Gain 1 honor" vs "Make opponent lose 1 honor") - AbilityExecutor threads that
    /// same choice through Execute/Prepare/Resolve as a plain parameter for JSON-driven
    /// abilities, but a Scripts class only receives AbilityContext, so it needs a home here
    /// too (asako-diplomat's own "Honor this character" vs "Dishonor this character").
    /// </summary>
    public string? ChosenChoice { get; set; }

    /// <summary>
    /// ringteki Duel's own winner (mirumoto-raitsugu/kakita-kaezin's duel-outcome-dependent
    /// follow-up) - a real duel resolution compares skill *and* the honor bid
    /// (Duel.getChallengerStatisticTotal/getTargetStatisticTotal), which this engine doesn't
    /// model. Same caller-set-fact convention as Conflict.Winner/Loser - the caller supplies
    /// which of the two duelists (context.Source as challenger, context.Target as the
    /// opponent's chosen character) won; the script derives the loser as "whichever one
    /// isn't this".
    /// </summary>
    public Card? DuelWinner { get; set; }

    /// <summary>
    /// ready-for-battle's own trigger condition: "event.context.source.type === 'ring' ||
    /// event.context.player === context.player.opponent" - was the bow that just happened
    /// caused by something other than the reacting player's own ability (a ring effect or
    /// the opponent), as opposed to the player bowing their own character as a cost. This
    /// engine has no event bus recording which ability/player caused a given bow, so (like
    /// every other event-shaped script this session) the caller sets this fact directly;
    /// false until then.
    /// </summary>
    public bool BowCausedBySelf { get; set; }

    /// <summary>
    /// shameful-display's own two-target selection (choose 2 participating characters,
    /// honor one and dishonor the other) - a script has only the single Target/CostTarget
    /// slots to work with otherwise, neither of which fits "two independent, differently-
    /// treated targets chosen together". The caller supplies the character to honor via
    /// Target and the character to dishonor via SecondTarget, same trust-the-caller
    /// convention as every other target field.
    /// </summary>
    public Card? SecondTarget { get; set; }
}
