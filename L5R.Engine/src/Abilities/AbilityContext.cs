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
}
