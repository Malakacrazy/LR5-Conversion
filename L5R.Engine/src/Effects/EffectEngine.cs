namespace L5R.Engine.Effects;

/// <summary>
/// Ports ringteki's EffectEngine (server/game/effectengine.js, checkEffects): conditional
/// persistent effects are recalculated in a fixed-point loop until the game state
/// stabilizes (an effect applying can flip another effect's condition, which can flip
/// another, etc).
///
/// Deliberate deviation from the original: ringteki throws once the loop hits its cap
/// (10 passes), which would crash the whole game process on a card-design bug where two
/// effects contradict each other forever. Since this engine's replay/spectator/
/// reconnection story depends on the game never dying mid-resolution, hitting the cap
/// here stops the loop and returns the current (still-oscillating) state as final for
/// this pass instead, surfaced via <see cref="RecalculationCapped"/> so tests and the
/// canonical event log can flag it as the bug it is without taking the game down.
/// </summary>
public sealed class EffectEngine
{
    private const int MaxIterations = 10;

    private readonly List<IRecalculableEffect> _effects = new();
    private bool _newEffect;

    /// <summary>True if the most recent CheckEffects() call hit the iteration cap without stabilizing.</summary>
    public bool RecalculationCapped { get; private set; }

    public void Add(IRecalculableEffect effect)
    {
        _effects.Add(effect);
        _newEffect = true;
    }

    /// <summary>
    /// Recalculates all registered effects until the state stabilizes or the iteration
    /// cap is hit. No-ops (returns false) unless <paramref name="force"/> is set or an
    /// effect was added since the last check — callers (the future game loop) are
    /// expected to pass force: true after any state-mutating event, mirroring how
    /// ringteki's game.js drives checkEffects from many event handlers, not just from Add().
    /// </summary>
    public bool CheckEffects(bool force = false)
    {
        RecalculationCapped = false;

        if (!force && !_newEffect)
            return false;

        _newEffect = false;

        bool changedAcrossAnyPass = false;
        for (int pass = 0; pass < MaxIterations; pass++)
        {
            bool passChanged = false;
            foreach (var effect in _effects)
                passChanged = effect.Recalculate() || passChanged;

            if (!passChanged)
                return changedAcrossAnyPass;

            changedAcrossAnyPass = true;

            if (pass == MaxIterations - 1)
                RecalculationCapped = true;
        }

        return changedAcrossAnyPass;
    }
}
