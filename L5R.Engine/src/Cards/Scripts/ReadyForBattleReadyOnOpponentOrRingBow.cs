namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for ready-for-battle: after an opponent's card effect or a ring effect
/// bows a character you control, ready that character. Needs event.card.controller and
/// event.context field inspection, plus targeting event.card directly rather than a
/// player-chosen selection. Stubbed until the state model has an effect-source-tracking
/// event shape.
/// </summary>
public sealed class ReadyForBattleReadyOnOpponentOrRingBow : ICardScript
{
}
