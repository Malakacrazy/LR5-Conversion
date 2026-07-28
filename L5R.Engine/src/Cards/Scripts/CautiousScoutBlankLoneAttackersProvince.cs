namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for cautious-scout: blanks the current conflict's province while this
/// character is attacking alone. Needs a direct reference to the conflict's province
/// object and a participant count, both beyond the closed predicate vocabulary. Stubbed
/// until the state model has conflicts.
/// </summary>
public sealed class CautiousScoutBlankLoneAttackersProvince : ICardScript
{
}
