using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// young-rumormonger: when a character would be honored or dishonored, choose a different
/// character controlled by the same player to receive it instead. Same "no new mechanism
/// needed" reasoning as reprieve/stand-your-ground - the caller calls this script instead
/// of honoring/dishonoring the original target. context.Target carries the original
/// (redirected-away-from) target, used only to check the new target shares its controller;
/// context.SecondTarget (shameful-display's own two-target field) carries the new target
/// that actually receives the honor/dishonor; context.ChosenChoice ("Honor"/"Dishonor",
/// same convention as court-games/asako-diplomat) selects which one, matching whichever
/// event actually fired. HonorGameActionHandler/DishonorGameActionHandler are reused
/// completely untouched.
/// </summary>
public sealed class YoungRumormongerRedirectHonorOrDishonor : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var rumormonger = context.Source;

        var originalTarget = context.Target
            ?? throw new InvalidOperationException($"'{rumormonger.Id}' requires context.Target (the original target) to be set.");

        var newTarget = context.SecondTarget
            ?? throw new InvalidOperationException($"'{rumormonger.Id}' requires context.SecondTarget (the new target) to be set.");

        if (newTarget == originalTarget)
            throw new InvalidOperationException($"'{rumormonger.Id}' requires a different character than '{originalTarget.Id}'.");

        if (newTarget.Controller != originalTarget.Controller)
            throw new InvalidOperationException($"'{newTarget.Id}' must be controlled by the same player as '{originalTarget.Id}'.");

        context.Target = newTarget;

        switch (context.ChosenChoice)
        {
            case "Honor":
                new HonorGameActionHandler().Execute(context, null);
                break;
            case "Dishonor":
                new DishonorGameActionHandler().Execute(context, null);
                break;
            default:
                throw new InvalidOperationException($"'{rumormonger.Id}' requires context.ChosenChoice to be 'Honor' or 'Dishonor'.");
        }
    }
}
