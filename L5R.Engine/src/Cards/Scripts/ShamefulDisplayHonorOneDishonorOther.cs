using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// shameful-display: choose 2 participating characters, honor one and dishonor the
/// other. Needs a bespoke multi-step selection with several eligibility fallback paths
/// (both must be eligible for either action, one might only be eligible for one, etc.),
/// far beyond a plain multi-select target - matches this card's own scriptOverride reason.
/// The fallback-prompt branches are UI-only concerns this engine's trust-the-caller
/// convention already sidesteps everywhere else: the caller supplies the final legal
/// assignment directly via context.Target (honor) / context.SecondTarget (dishonor).
/// </summary>
public sealed class ShamefulDisplayHonorOneDishonorOther : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var shamefulDisplay = context.Source;

        var honorTarget = context.Target
            ?? throw new InvalidOperationException($"'{shamefulDisplay.Id}' requires context.Target (the character to honor) to be set.");
        var dishonorTarget = context.SecondTarget
            ?? throw new InvalidOperationException($"'{shamefulDisplay.Id}' requires context.SecondTarget (the character to dishonor) to be set.");

        if (honorTarget == dishonorTarget)
            throw new InvalidOperationException($"'{shamefulDisplay.Id}' requires two different characters.");

        if (!IsParticipating(context.Game, honorTarget))
            throw new InvalidOperationException($"'{honorTarget.Id}' is not participating.");
        if (!IsParticipating(context.Game, dishonorTarget))
            throw new InvalidOperationException($"'{dishonorTarget.Id}' is not participating.");

        context.Target = honorTarget;
        new HonorGameActionHandler().Execute(context, null);

        context.Target = dishonorTarget;
        new DishonorGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
