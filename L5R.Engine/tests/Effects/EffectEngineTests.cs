using L5R.Engine.Effects;

namespace L5R.Engine.Tests.Effects;

/// <summary>
/// Spike tests for the effect engine port (ringteki effectengine.js checkEffects). Gates
/// card porting alongside the scheduler spike.
/// </summary>
public class EffectEngineTests
{
    [Test]
    public void StaticEffect_AppliesOnceAndStopsReportingChange()
    {
        var engine = new EffectEngine();
        var effect = new ToggleEffect(() => true);
        engine.Add(effect);

        var firstCheck = engine.CheckEffects();
        Assert.That(firstCheck, Is.True);
        Assert.That(effect.IsApplied, Is.True);

        var secondCheck = engine.CheckEffects(force: true);
        Assert.That(secondCheck, Is.False, "nothing left to stabilize once applied and condition still holds");
        Assert.That(engine.RecalculationCapped, Is.False);
    }

    [Test]
    public void ConditionalEffect_TurnsOnAndOffAsItsConditionChanges()
    {
        // e.g. "while this character is participating, +1 skill" - the condition is
        // driven by external game state, not by the effect's own application.
        bool participating = false;
        var engine = new EffectEngine();
        var buff = new ToggleEffect(() => participating);
        engine.Add(buff);

        engine.CheckEffects();
        Assert.That(buff.IsApplied, Is.False, "condition was false when the effect was added");

        participating = true;
        Assert.That(engine.CheckEffects(force: true), Is.True);
        Assert.That(buff.IsApplied, Is.True);

        participating = false;
        Assert.That(engine.CheckEffects(force: true), Is.True);
        Assert.That(buff.IsApplied, Is.False);
    }

    [Test]
    public void ContradictingEffects_StopAtTheIterationCapWithADeterministicResult()
    {
        // Effect1 wants to apply only while Effect2 hasn't; Effect2 wants to apply only
        // once Effect1 has. Each one's application invalidates the other's condition -
        // a genuine infinite flip-flop that only the iteration cap can end.
        (bool a, bool b) RunToCompletion()
        {
            var flags = new MutualFlags();
            var engine = new EffectEngine();
            engine.Add(new FlagEffect(
                condition: () => !flags.B,
                apply: () => flags.A = true,
                unapply: () => flags.A = false));
            engine.Add(new FlagEffect(
                condition: () => flags.A,
                apply: () => flags.B = true,
                unapply: () => flags.B = false));

            Assert.That(() => engine.CheckEffects(), Throws.Nothing, "must not crash the game on a contradicting-effects bug");
            Assert.That(engine.RecalculationCapped, Is.True, "the cap must be the reason it stopped, not accidental convergence");

            return (flags.A, flags.B);
        }

        var firstRun = RunToCompletion();
        var secondRun = RunToCompletion();

        Assert.That(secondRun, Is.EqualTo(firstRun), "same starting state must cap out at the same final state every time");
    }

    private sealed class ToggleEffect : IRecalculableEffect
    {
        private readonly Func<bool> _condition;

        public ToggleEffect(Func<bool> condition) => _condition = condition;

        public bool IsApplied { get; private set; }

        public bool Recalculate()
        {
            bool shouldBeApplied = _condition();
            if (shouldBeApplied == IsApplied)
                return false;

            IsApplied = shouldBeApplied;
            return true;
        }
    }

    private sealed class FlagEffect : IRecalculableEffect
    {
        private readonly Func<bool> _condition;
        private readonly Action _apply;
        private readonly Action _unapply;
        private bool _applied;

        public FlagEffect(Func<bool> condition, Action apply, Action unapply)
        {
            _condition = condition;
            _apply = apply;
            _unapply = unapply;
        }

        public bool Recalculate()
        {
            bool shouldBeApplied = _condition();
            if (shouldBeApplied == _applied)
                return false;

            _applied = shouldBeApplied;
            if (_applied)
                _apply();
            else
                _unapply();
            return true;
        }
    }

    private sealed class MutualFlags
    {
        public bool A;
        public bool B;
    }
}
