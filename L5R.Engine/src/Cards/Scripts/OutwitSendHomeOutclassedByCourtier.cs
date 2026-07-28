namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for outwit: during a conflict, move an opponent's character with lower
/// political skill than a participating Courtier you control home. Needs a
/// double-candidate comparison (target vs. an existentially-checked courtier) beyond
/// anyCardMatches's single local candidate. Stubbed until the state model has conflicts.
/// </summary>
public sealed class OutwitSendHomeOutclassedByCourtier : ICardScript
{
}
