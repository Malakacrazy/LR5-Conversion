using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// The player-scoped sibling of cardLastingEffect: applies an effect to a player instead
/// of a card, for a limited duration. Same duration handling (default
/// "untilEndOfConflict", only that and "untilEndOfPhase" supported).
///
/// "targetController" (default "self") picks the recipient player directly - there's no
/// bulk "target" override like cardLastingEffect's allCardsMatching, since a player-scoped
/// effect only ever has one or two possible recipients (self/opponent), not a set of cards
/// to filter.
/// </summary>
public sealed class PlayerLastingEffectGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (parameters is null)
            throw new InvalidOperationException("playerLastingEffect requires params (duration and effect).");

        var props = parameters.Value;

        var duration = props.TryGetProperty("duration", out var durationElement) ? durationElement.GetString()! : "untilEndOfConflict";
        if (duration != "untilEndOfPhase" && duration != "untilEndOfConflict")
            throw new NotSupportedException($"PlayerLastingEffectGameActionHandler does not yet support duration '{duration}'.");

        var targetController = props.TryGetProperty("targetController", out var tc) ? tc.GetString()! : "self";
        var recipient = targetController switch
        {
            "self" => context.Player,
            "opponent" => context.Game.Opponent(context.Player),
            _ => throw new NotSupportedException($"PlayerLastingEffectGameActionHandler does not yet support targetController '{targetController}'.")
        };

        var effect = props.GetProperty("effect");
        var effectName = effect.GetProperty("name").GetString();
        var effectValue = effect.TryGetProperty("value", out var v) ? v : (JsonElement?)null;

        if (EffectVocabulary.TryGetPlayerRestrictionAction(effectName, effectValue, out var action, out var qualifier))
        {
            context.Game.PlayerRestrictions.Add(new PlayerRestriction { Target = recipient, Action = action, Source = context.Source, Qualifier = qualifier, Duration = duration });
            return;
        }

        if (EffectVocabulary.TryGetCostReduction(effectName, effectValue, out var amount, out var appliesTo))
        {
            context.Game.CostReductions.Add(new PlayerCostReduction { Player = recipient, Amount = amount, AppliesTo = appliesTo, Duration = duration });
            return;
        }

        throw new NotSupportedException($"PlayerLastingEffectGameActionHandler does not yet support effect '{effectName}'.");
    }
}
