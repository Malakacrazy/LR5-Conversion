using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Abilities;

/// <summary>
/// Ports the requirement-checking chain from ringteki's CardAction.meetsRequirements
/// and its base BaseAbility.meetsRequirements: location -> phase -> player -> ability
/// limit -> condition -> costs affordable -> legal targets exist.
/// </summary>
public sealed class CardAction
{
    public required string Title { get; init; }
    public required Card Card { get; init; }

    /// <summary>
    /// The parsed abilities.actions[] entry this CardAction bridges from JSON
    /// (CardFactory), null for hand-built CardActions like LegalActionsTests' own. Once
    /// MeetsRequirements passes, the caller resolves the action by calling
    /// AbilityExecutor.Execute(Definition, context, ...) directly - this field exists purely
    /// so the game loop doesn't have to re-parse or re-locate the JSON it came from.
    /// </summary>
    public ActionDefinition? Definition { get; init; }

    /// <summary>ringteki CardAction.buildLocation; defaults to 'play area'.</summary>
    public IReadOnlyList<string> ValidLocations { get; init; } = new[] { "play area" };

    /// <summary>Null means ringteki's phase: 'any'.</summary>
    public Phase? Phase { get; init; }

    public bool AnyPlayer { get; init; }
    public Func<AbilityContext, bool>? Condition { get; init; }
    public IReadOnlyList<ICost> Costs { get; init; } = Array.Empty<ICost>();
    public IReadOnlyList<ITargetRequirement> Targets { get; init; } = Array.Empty<ITargetRequirement>();
    public IAbilityLimit Limit { get; init; } = UnlimitedAbilityLimit.Instance;

    public bool MeetsRequirements(AbilityContext context)
    {
        if (!ValidLocations.Contains(context.Source.Location))
            return false;

        if (Phase.HasValue && Phase.Value != context.Game.CurrentPhase)
            return false;

        if (!AnyPlayer && context.Player != context.Source.Controller)
            return false;

        if (Limit.IsAtMax(context.Player))
            return false;

        if (Condition is not null && !Condition(context))
            return false;

        if (!Costs.All(cost => cost.CanPay(context)))
            return false;

        return Targets.All(target => target.HasLegalTarget(context));
    }
}
