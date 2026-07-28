namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for obstinate-recruit: discard this character if an opponent is more
/// honorable than you. Needs a delayedEffect (a self-monitoring nested condition+
/// gameAction) inside a persistentEffect, a shape the schema's flat persistentEffect
/// fields can't hold. Stubbed until the state model has honor tracking.
/// </summary>
public sealed class ObstinateRecruitDiscardWhenOpponentMoreHonorable : ICardScript
{
}
