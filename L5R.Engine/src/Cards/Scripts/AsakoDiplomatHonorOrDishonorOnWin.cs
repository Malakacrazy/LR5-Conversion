using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// asako-diplomat: after this character wins a conflict it's participating in, choose a
/// character to honor or dishonor (ringteki's "chooseAction" - the same "mode": "select"
/// shape city-of-the-open-hand already uses generically, except gated on
/// "event.conflict.winner"). The caller supplies both the target character (context.Target)
/// and the chosen label (context.ChosenChoice, new - a script has no other way to receive a
/// labeled choice the way AbilityExecutor's own chosenChoice parameter does for JSON-driven
/// abilities). Both branches reuse the existing Honor/DishonorGameActionHandler directly.
/// </summary>
public sealed class AsakoDiplomatHonorOrDishonorOnWin : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var diplomat = context.Source;

        if (context.Game.CurrentConflict?.Winner != context.Player)
            throw new InvalidOperationException($"'{diplomat.Id}' can only trigger when its controller wins the conflict.");

        if (!IsParticipating(context.Game, diplomat))
            throw new InvalidOperationException($"'{diplomat.Id}' can only trigger while participating.");

        if (context.Target is null)
            throw new InvalidOperationException($"'{diplomat.Id}' requires context.Target to be set.");

        switch (context.ChosenChoice)
        {
            case "Honor this character":
                new HonorGameActionHandler().Execute(context, null);
                break;
            case "Dishonor this character":
                new DishonorGameActionHandler().Execute(context, null);
                break;
            default:
                throw new InvalidOperationException($"'{diplomat.Id}' requires context.ChosenChoice to be 'Honor this character' or 'Dishonor this character'.");
        }
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
