namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for display-of-power: after losing an unopposed conflict, cancel that
/// conflict's ring effect, resolve it as if you had won as attacker, then claim the ring.
/// Needs event.conflict field inspection and a bespoke one-shot interrupt-cancel handler
/// far beyond a gameAction. Stubbed until the state model has conflicts and rings.
/// </summary>
public sealed class DisplayOfPowerCancelAndClaimRing : ICardScript
{
}
