namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for blackmail-artist: after this character wins a political conflict,
/// take 1 honor from your opponent. Needs event.conflict.winner/conflictType field
/// inspection beyond the closed predicate vocabulary. Stubbed until the state model has
/// conflicts.
/// </summary>
public sealed class BlackmailArtistTakeHonorOnPoliticalWin : ICardScript
{
}
