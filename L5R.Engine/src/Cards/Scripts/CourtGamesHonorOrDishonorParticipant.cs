using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// court-games: during a political conflict, either honor a participating character its
/// controller controls or dishonor a participating character the opponent controls. Needs
/// a two-level select (choose the action, then choose a card for it) - target.choices only
/// supports a plain predicate/gameAction leaf value, not a full nested card selection
/// (matches this card's own scriptOverride reason). context.ChosenChoice carries the
/// chosen label, same convention as asako-diplomat/ide-trader; context.Target carries the
/// chosen character either way.
/// </summary>
public sealed class CourtGamesHonorOrDishonorParticipant : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var courtGames = context.Source;

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{courtGames.Id}' requires an active conflict.");

        if (conflict.ConflictType != "political")
            throw new InvalidOperationException($"'{courtGames.Id}' can only be used during a political conflict.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{courtGames.Id}' requires context.Target to be set.");

        if (!IsParticipating(context.Game, target))
            throw new InvalidOperationException($"'{target.Id}' is not participating.");

        switch (context.ChosenChoice)
        {
            case "Honor a friendly character":
                if (target.Controller != context.Player)
                    throw new InvalidOperationException($"'{target.Id}' must be controlled by '{courtGames.Id}''s controller.");
                new HonorGameActionHandler().Execute(context, null);
                break;
            case "Dishonor an opposing character":
                if (target.Controller != context.Game.Opponent(context.Player))
                    throw new InvalidOperationException($"'{target.Id}' must be controlled by the opponent.");
                new DishonorGameActionHandler().Execute(context, null);
                break;
            default:
                throw new InvalidOperationException($"'{courtGames.Id}' requires context.ChosenChoice to be 'Honor a friendly character' or 'Dishonor an opposing character'.");
        }
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
