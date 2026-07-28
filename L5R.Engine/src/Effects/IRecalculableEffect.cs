namespace L5R.Engine.Effects;

/// <summary>
/// One persistent/lasting effect instance as seen by <see cref="EffectEngine"/>'s
/// fixed-point recalculation loop. Mirrors the role of Effect.checkCondition in
/// ringteki's server/game/Effects/effect.js: re-evaluate whether this effect's condition
/// currently holds and apply/unapply accordingly.
/// </summary>
public interface IRecalculableEffect
{
    /// <summary>
    /// Re-evaluates this effect against current game state, applying or unapplying it if
    /// its condition changed. Returns true if anything changed (forcing another pass).
    /// </summary>
    bool Recalculate();
}
