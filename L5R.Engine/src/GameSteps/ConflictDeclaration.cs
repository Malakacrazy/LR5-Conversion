using L5R.Engine.State;

namespace L5R.Engine.GameSteps;

/// <summary>
/// An IBotPolicy's decision to declare a conflict: which ring (also fixes ConflictType,
/// since a ring's own ConflictType is "military" or "political"), which province (or a
/// player's Stronghold, once eligible - see ConflictResolver.AttackableProvinces), and
/// which of the attacker's own untapped characters commit as attackers. Mirrors ringteki's
/// InitiateConflictPrompt's three choices; no covert/ring-switching complexity modeled (see
/// ConflictResolver's own doc comment for what's deliberately out of scope for v1).
/// </summary>
public sealed record ConflictDeclaration(string Ring, Card Province, IReadOnlyList<Card> Attackers);
