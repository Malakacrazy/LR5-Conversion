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

        var recipients = props.TryGetProperty("target", out var targetElement)
            ? TargetResolver.ResolveAllCardsMatching(targetElement, context)
            : new[] { context.Target ?? throw new InvalidOperationException("cardLastingEffect requires context.Target to be set.") };

        if (EffectVocabulary.TryGetRestrictionAction(effectName, effectValue, out var action, out var qualifier))
        {
            foreach (var recipient in recipients)
                context.Game.Restrictions.Add(new CardRestriction { Target = recipient, Action = action, Duration = duration, Qualifier = qualifier });
            return;
        }

        if (effectName == "takeControl")
        {
            foreach (var recipient in recipients)
                context.Game.TakeControl(recipient, context.Player, duration);
            return;
        }

        var value = ValueRefResolver.ResolveInt(effectValue!.Value, context);
        if (!EffectVocabulary.TryGetStatDeltas(effectName, value, out var deltas))
            throw new NotSupportedException($"CardLastingEffectGameActionHandler does not yet support effect '{effectName}'.");

        foreach (var recipient in recipients)
            foreach (var (stat, delta) in deltas)
                context.Game.LastingEffects.Add(new LastingEffect { Target = recipient, Stat = stat, Value = delta, Duration = duration });
    }
}
