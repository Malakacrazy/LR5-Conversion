namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for spyglass: after the attached character commits to a conflict or
/// moves to a conflict, draw 1 card (limit twice per round). Needs array-membership
/// checks (source.parent in event.attackers/event.defenders) with no equivalent in the
/// closed predicate vocabulary, plus a per-round reaction limit. Stubbed until the state
/// model has conflicts.
/// </summary>
public sealed class SpyglassDrawOnParentJoiningConflict : ICardScript
{
}
