namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for shinjo-tatsuo: during a conflict, move this character and up to 1
/// other character you control to the conflict. Needs a per-target optional flag within
/// a targets map, which target.mode's upTo/exactly (single-target cardinality) doesn't
/// express. Stubbed until the state model has conflicts.
/// </summary>
public sealed class ShinjoTatsuoMoveSelfAndOptionalAllyToConflict : ICardScript
{
}
