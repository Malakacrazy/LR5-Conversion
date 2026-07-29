using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// ide-trader: while this character is participating, choose to gain 1 fate or draw 1
/// card (ringteki's collectiveTrigger/perConflict-limit batching a set of simultaneous
/// onMoveToConflict events into a single reaction - not modeled here since every ported
/// script is triggered once per Execute call anyway; "max: perConflict(1)" needs no work,
/// matching every other "max"/"limit" field's established no-op precedent). The caller
/// supplies the chosen label via context.ChosenChoice, same convention as asako-diplomat.
/// </summary>
public sealed class IdeTraderGainFateOrDrawOnAllyMovingToConflict : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var trader = context.Source;

        if (!IsParticipating(context.Game, trader))
            throw new InvalidOperationException($"'{trader.Id}' can only trigger while participating.");

        switch (context.ChosenChoice)
        {
            case "Gain 1 fate":
                new GainFateGameActionHandler().Execute(context, null);
                break;
            case "Draw 1 card":
                new DrawGameActionHandler().Execute(context, null);
                break;
            default:
                throw new InvalidOperationException($"'{trader.Id}' requires context.ChosenChoice to be 'Gain 1 fate' or 'Draw 1 card'.");
        }
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
