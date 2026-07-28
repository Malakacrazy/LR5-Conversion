namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for forgotten-library: after the draw phase begins, draw 1 card. Needs
/// event.phase field inspection, a scalar event field not covered by the event.card
/// 'isSelf' when convention. Stubbed until the state model has phases.
/// </summary>
public sealed class ForgottenLibraryDrawOnDrawPhase : ICardScript
{
}
