namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for steadfast-samurai: forced reaction protecting the card from being
/// discarded or losing fate during the fate phase, gated on the controller's honor being
/// at least 5 more than their opponent's. Stubbed until the state model has phases and
/// per-player honor tracked.
/// </summary>
public sealed class SteadfastSamuraiHonorThresholdProtection : ICardScript
{
}
