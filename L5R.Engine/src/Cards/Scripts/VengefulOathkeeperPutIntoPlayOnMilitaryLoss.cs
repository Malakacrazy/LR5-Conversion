namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for vengeful-oathkeeper: after losing a military conflict, put this
/// character into play from your hand. Needs event.conflict.loser/conflictType field
/// inspection beyond the closed predicate vocabulary. Stubbed until the state model has
/// conflicts.
/// </summary>
public sealed class VengefulOathkeeperPutIntoPlayOnMilitaryLoss : ICardScript
{
}
