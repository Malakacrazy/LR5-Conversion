namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for for-greater-glory: after breaking a province during a military
/// conflict, place 1 fate on each bushi character on your side (max 1 per conflict).
/// Needs event.conflict field inspection and a conflict-collection query beyond the
/// closed predicate vocabulary. Stubbed until the state model has conflicts.
/// </summary>
public sealed class ForGreaterGloryPlaceFateOnBushi : ICardScript
{
}
