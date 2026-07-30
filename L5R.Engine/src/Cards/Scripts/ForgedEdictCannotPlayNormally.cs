using L5R.Engine.Abilities;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// forged-edict: its own effect is entirely a JSON-driven "wouldInterrupt" triggeredAbility
/// (see its own card JSON), resolved exclusively by WouldInterruptOfferer at the exact
/// moment another event initiates its effects - never through the generic hand-play path,
/// which has no bridged Card.Actions entry and no ScriptedActionRegistry registration for
/// this card either. Without this CanPlay override, LegalActions.GetLegalPlays would still
/// offer it as an ordinary hand play (it only checks affordability/type, not a card's real
/// trigger condition), and a bot would discard it with no effect the moment it's affordable,
/// wasting it before WouldInterruptOfferer ever gets a chance. Execute is never called on
/// this script - only CanPlay is consulted, the same narrow wiring point good-omen/blackmail
/// already use for the opposite purpose (restricting when a JSON-driven effect can be played
/// at all, not blocking it entirely).
/// </summary>
public sealed class ForgedEdictCannotPlayNormally : ICardScript
{
    public bool CanPlay(AbilityContext context) => false;
}
