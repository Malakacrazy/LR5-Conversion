using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki LastingEffectCardAction (GameActions.ts cardLastingEffect): applies a stat
/// modifier to context.Target for a limited duration. LastingEffectCardProperties defaults
/// duration to Durations.UntilEndOfConflict when omitted - unsupported here (no Conflict
/// object exists yet to know when it ends), so only an explicit "untilEndOfPhase" is
/// accepted; anything else throws rather than silently applying the wrong duration.
/// Only "modifyGlory" is understood so far - the other modify*Skill effect names need
/// Military/Political stats this engine doesn't have yet.
/// </summary>
public sealed class CardLastingEffectGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("cardLastingEffect requires context.Target to be set.");

        if (parameters is null)
            throw new InvalidOperationException("cardLastingEffect requires params (duration and effect).");

        var props = parameters.Value;

        var duration = props.TryGetProperty("duration", out var durationElement) ? durationElement.GetString() : "untilEndOfConflict";
        if (duration != "untilEndOfPhase")
            throw new NotSupportedException($"CardLastingEffectGameActionHandler does not yet support duration '{duration}' (needs conflict/round-end tracking).");

        var effect = props.GetProperty("effect");
        var effectName = effect.GetProperty("name").GetString();
        if (effectName != "modifyGlory")
            throw new NotSupportedException($"CardLastingEffectGameActionHandler does not yet support effect '{effectName}'.");

        var value = effect.GetProperty("value").GetInt32();

        context.Game.LastingEffects.Add(new LastingEffect { Target = context.Target, Stat = "glory", Value = value });
    }
}
