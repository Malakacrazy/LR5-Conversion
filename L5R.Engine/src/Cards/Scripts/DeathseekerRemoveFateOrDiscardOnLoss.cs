namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for deathseeker: after this character loses a conflict as attacker,
/// sacrifice it and either remove 1 fate from or discard an opponent's character
/// depending on whether it has fate on it. Needs event.conflict.loser field inspection
/// and a bespoke conditional handler ringteki itself has no generic action for. Stubbed
/// until the state model has conflicts.
/// </summary>
public sealed class DeathseekerRemoveFateOrDiscardOnLoss : ICardScript
{
}
