namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for banzai: during a conflict, give a participating character +2
/// military until the end of the conflict; may lose 1 honor to resolve again (max 1 per
/// conflict overall, but repeatable via the honor cost). Needs a recursive 'then' chain
/// with per-iteration prompt text, a bespoke stateful interaction. Stubbed until the
/// state model has conflicts.
/// </summary>
public sealed class BanzaiGrantMilitarySkillRepeatable : ICardScript
{
}
