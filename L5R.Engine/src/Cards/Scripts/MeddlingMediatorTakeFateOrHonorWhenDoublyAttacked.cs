namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for meddling-mediator: if an opponent has declared 2+ conflicts
/// against you this phase, take 1 fate or 1 honor from them. Needs a count of the
/// opponent's remaining conflict declarations this phase, a conflict-collection query
/// beyond the closed predicate vocabulary. Stubbed until the state model has a
/// conflict-declaration record.
/// </summary>
public sealed class MeddlingMediatorTakeFateOrHonorWhenDoublyAttacked : ICardScript
{
}
