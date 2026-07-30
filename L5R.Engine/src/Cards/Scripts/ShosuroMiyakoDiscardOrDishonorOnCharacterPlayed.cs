using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// shosuro-miyako: after its controller plays a character from hand, the opponent
/// discards a card at random or dishonors a character they control. "event.player ===
/// context.player" is inherently about context.Player's own play, and neither branch
/// references the played card itself - same no-eventCard-needed, caller-asserts-it-
/// happened convention as watch-commander. context.ChosenChoice carries the chosen
/// branch, same convention as asako-diplomat/ide-trader/court-games.
/// </summary>
public sealed class ShosuroMiyakoDiscardOrDishonorOnCharacterPlayed : ICardScript
{
    private static readonly JsonElement AmountOne = JsonDocument.Parse("{\"amount\":1}").RootElement;

    public void Execute(AbilityContext context)
    {
        var miyako = context.Source;

        switch (context.ChosenChoice)
        {
            case "Discard at random":
                new ChosenDiscardGameActionHandler().Execute(context, AmountOne);
                break;
            case "Dishonor a character":
                var target = context.Target
                    ?? throw new InvalidOperationException($"'{miyako.Id}' requires context.Target to be set.");

                if (target.Controller != context.Game.Opponent(context.Player))
                    throw new InvalidOperationException($"'{target.Id}' must be controlled by the opponent.");

                new DishonorGameActionHandler().Execute(context, null);
                break;
            default:
                throw new InvalidOperationException($"'{miyako.Id}' requires context.ChosenChoice to be 'Discard at random' or 'Dishonor a character'.");
        }
    }
}
