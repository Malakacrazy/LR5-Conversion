namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for hida-kisada: while you haven't lost a conflict this phase, cancel
/// the effects of the first action ability your opponent triggers from a card during each
/// conflict. Needs raw event registration across multiple event/stage pairs plus mutable
/// per-round state tracking, far beyond a single triggeredAbility. Stubbed until the state
/// model has conflicts and a phase/round record.
/// </summary>
public sealed class HidaKisadaCancelOpponentsFirstActionEachConflict : ICardScript
{
}
