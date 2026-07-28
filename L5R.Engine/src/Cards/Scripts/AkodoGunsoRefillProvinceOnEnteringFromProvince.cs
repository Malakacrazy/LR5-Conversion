namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for akodo-gunso: after this character enters play from a province,
/// refill that province faceup. Needs event.originalLocation array-membership inspection
/// and referencing that field as a gameAction param, neither modeled by the closed
/// vocabulary. Stubbed until the state model has provinces.
/// </summary>
public sealed class AkodoGunsoRefillProvinceOnEnteringFromProvince : ICardScript
{
}
