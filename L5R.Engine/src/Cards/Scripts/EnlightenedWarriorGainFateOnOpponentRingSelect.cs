namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for enlightened-warrior: after an opponent selects a ring with fate on
/// it, place 1 fate on this character. Needs event.ringFate/event.conflict.attackingPlayer
/// field inspection beyond the closed predicate vocabulary. Stubbed until the state model
/// has conflicts and rings.
/// </summary>
public sealed class EnlightenedWarriorGainFateOnOpponentRingSelect : ICardScript
{
}
