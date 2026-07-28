namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for pilgrimage: during conflicts at this province, cancel all ring
/// effects (unless the province itself breaks). Needs raw event registration across two
/// event/stage pairs with a bespoke shared handler, far beyond a single triggeredAbility.
/// Stubbed until the state model has conflicts and rings.
/// </summary>
public sealed class PilgrimageCancelRingEffectsAtThisProvince : ICardScript
{
}
