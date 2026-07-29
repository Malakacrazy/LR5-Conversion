using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki LastingEffectCardAction (GameActions.ts cardLastingEffect): applies a stat
/// modifier to a set of recipient cards for a limited duration.
/// LastingEffectCardProperties defaults duration to Durations.UntilEndOfConflict when
/// omitted - every ported skill-boost card in the executable set so far actually relies on
/// that default (a combat trick lasting "until the end of the conflict" is the norm; only
/// isawa-mori-seido gives an explicit untilEndOfPhase), so both are accepted and stored on
/// the LastingEffect for GameState.EndConflict()/AdvancePhase() to expire correctly.
///
/// Recipients default to context.Target, but params can carry their own "target"
/// (allCardsMatching, same shape as a gameActionEntry's own target override) that applies
/// the effect to every matching card instead - e.g. shiro-nishiyama's "give defending
/// characters +1/+1" has no single chosen target at all, just a bulk scope.
/// </summary>
public sealed class CardLastingEffectGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (parameters is null)
            throw new InvalidOperationException("cardLastingEffect requires params (duration and effect).");

        var props = parameters.Value;

        var duration = props.TryGetProperty("duration", out var durationElement) ? durationElement.GetString()! : "untilEndOfConflict";
        if (duration != "untilEndOfPhase" && duration != "untilEndOfConflict")
            throw new NotSupportedException($"CardLastingEffectGameActionHandler does not yet support duration '{duration}'.");

        var effect = props.GetProperty("effect");
        var effectName = effect.GetProperty("name").GetString();
        var effectValue = effect.TryGetProperty("value", out var v) ? v : (JsonElement?)null;

        // the-mountain-does-not-fall/adept-of-the-waves: scopes when the effect applies
        // (e.g. "only while defending", "only during a water conflict"), distinct from
        // Duration (when it expires) - re-checked live by every consumer below.
        var condition = props.TryGetProperty("condition", out var conditionElement)
            ? conditionElement.Clone()
            : (JsonElement?)null;

        var recipients = props.TryGetProperty("target", out var targetElement)
            ? TargetResolver.ResolveAllCardsMatching(targetElement, context)
            : new[] { context.Target ?? throw new InvalidOperationException("cardLastingEffect requires context.Target to be set.") };

        if (EffectVocabulary.TryGetRestrictionAction(effectName, effectValue, out var action, out var qualifier, out _))
        {
            foreach (var recipient in recipients)
                context.Game.Restrictions.Add(new CardRestriction { Target = recipient, Action = action, Duration = duration, Qualifier = qualifier, Condition = condition });
            return;
        }

        if (effectName == "takeControl")
        {
            foreach (var recipient in recipients)
                context.Game.TakeControl(recipient, context.Player, duration);
            return;
        }

        // bayushi-yunako's switchBaseSkills - no "value" key at all (a bare toggle), so it's
        // dispatched before the value-carrying branches below. Reuses the LastingEffects
        // list with a sentinel Stat name rather than a whole new list: it never matches
        // "military"/"political" in EffectiveStat's own additive/multiplier sums, and
        // GameState.EffectiveMilitarySkill/EffectivePoliticalSkill check for it directly to
        // swap which printed value feeds the stat's base (ringteki getBaseSkillModifiers'
        // SwitchBaseSkills case).
        if (effectName == "switchBaseSkills")
        {
            foreach (var recipient in recipients)
                context.Game.LastingEffects.Add(new LastingEffect { Target = recipient, Stat = "switchBaseSkills", Value = 0, Duration = duration });
            return;
        }

        // adept-of-the-waves' addKeyword - a string value, so it's dispatched before the
        // int-only stat-effect branches below rather than falling through to ResolveInt.
        if (effectName == "addKeyword")
        {
            var keyword = effectValue!.Value.GetString()!;
            foreach (var recipient in recipients)
                context.Game.LastingKeywordGrants.Add(new LastingKeywordGrant { Target = recipient, Keyword = keyword, Duration = duration, Condition = condition });
            return;
        }

        var value = ValueRefResolver.ResolveInt(effectValue!.Value, context);

        if (EffectVocabulary.TryGetStatMultiplier(effectName, value, out var multiplierStat, out var multiplier))
        {
            foreach (var recipient in recipients)
                context.Game.LastingEffects.Add(new LastingEffect { Target = recipient, Stat = multiplierStat, Value = 0, Multiplier = multiplier, Duration = duration });
            return;
        }

        if (!EffectVocabulary.TryGetStatDeltas(effectName, value, out var deltas))
            throw new NotSupportedException($"CardLastingEffectGameActionHandler does not yet support effect '{effectName}'.");

        foreach (var recipient in recipients)
            foreach (var (stat, delta) in deltas)
                context.Game.LastingEffects.Add(new LastingEffect { Target = recipient, Stat = stat, Value = delta, Duration = duration });
    }
}
