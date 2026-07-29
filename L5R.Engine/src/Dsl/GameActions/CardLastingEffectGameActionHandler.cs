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

        var recipients = props.TryGetProperty("target", out var targetElement)
            ? TargetResolver.ResolveAllCardsMatching(targetElement, context)
            : new[] { context.Target ?? throw new InvalidOperationException("cardLastingEffect requires context.Target to be set.") };

        if (effectName == "cardCannot")
        {
            // {"cannot": "..."} is the only value shape ported so far - a bare string value
            // (hiruma-yojimbo) or an object with a "restricts" sibling both exist in
            // ringteki but no card in the executable set needs them yet.
            var action = effect.GetProperty("value").GetProperty("cannot").GetString()!;
            foreach (var recipient in recipients)
                context.Game.Restrictions.Add(new CardRestriction { Target = recipient, Action = action, Duration = duration });
            return;
        }

        var value = ValueRefResolver.ResolveInt(effect.GetProperty("value"), context);
        foreach (var recipient in recipients)
            AddEffect(context, recipient, effectName, value, duration);
    }

    private static void AddEffect(AbilityContext context, Card recipient, string? effectName, int value, string duration)
    {
        switch (effectName)
        {
            case "modifyGlory":
                context.Game.LastingEffects.Add(new LastingEffect { Target = recipient, Stat = "glory", Value = value, Duration = duration });
                break;
            case "modifyMilitarySkill":
                context.Game.LastingEffects.Add(new LastingEffect { Target = recipient, Stat = "military", Value = value, Duration = duration });
                break;
            case "modifyPoliticalSkill":
                context.Game.LastingEffects.Add(new LastingEffect { Target = recipient, Stat = "political", Value = value, Duration = duration });
                break;
            case "modifyBothSkills":
                context.Game.LastingEffects.Add(new LastingEffect { Target = recipient, Stat = "military", Value = value, Duration = duration });
                context.Game.LastingEffects.Add(new LastingEffect { Target = recipient, Stat = "political", Value = value, Duration = duration });
                break;
            default:
                throw new NotSupportedException($"CardLastingEffectGameActionHandler does not yet support effect '{effectName}'.");
        }
    }
}
