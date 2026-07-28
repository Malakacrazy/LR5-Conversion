namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for ide-trader: after 1 or more characters move to a conflict this
/// character is participating in, gain 1 fate or draw 1 card (limit once per conflict).
/// Needs collectiveTrigger (batch a set of simultaneous events into one trigger) and a
/// per-conflict reaction limit, neither modeled by the triggeredAbility schema. Stubbed
/// until the state model has conflicts.
/// </summary>
public sealed class IdeTraderGainFateOrDrawOnAllyMovingToConflict : ICardScript
{
}
