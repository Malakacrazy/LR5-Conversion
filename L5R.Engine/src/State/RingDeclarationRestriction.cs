namespace L5R.Engine.State;

/// <summary>
/// way-of-the-phoenix's own "cannotDeclareRing" - a ring-scoped, player-scoped restriction
/// preventing a player from declaring a conflict with a given ring element for the rest of
/// the phase. Queryable via GameState.CannotDeclareRingWith rather than an enforced
/// pipeline, same precedent as IsAttachRestricted/IsRestrictedFrom - there's no generic
/// "declare a conflict" action anywhere in this engine to wire this into automatically.
/// </summary>
public sealed class RingDeclarationRestriction
{
    public required Player Player { get; init; }
    public required string Element { get; init; }
    public required string Duration { get; init; }
}
