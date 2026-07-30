using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// duelist-training: while attached, its host may initiate a military duel against a
/// participating enemy character; the lower (or tied) bidder pays the difference between
/// the two honor bids (Player.ShowBid, already tracked for other cards this session),
/// either by transferring that much honor to the other player or by discarding that many
/// cards from their own hand, then the duel's loser bows. Neither existing honor-transfer
/// nor discard handler fits this cost's shape (TakeHonorGameActionHandler's target is
/// always context.Player's fixed opponent; the low bidder here could be either player, and
/// they discard their *own* hand, not the opponent's like ChosenDiscardGameActionHandler),
/// so both payment branches are inlined directly. context.DuelWinner resolves the duel
/// itself, same caller-set-fact convention as mirumoto-raitsugu/kakita-kaezin - no
/// duel-resolution pipeline (skill + honor bid comparison) exists to compute it
/// automatically.
/// </summary>
public sealed class DuelistTrainingGrantMilitaryDuelAction : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var host = context.Source;

        if (!IsParticipating(context.Game, host))
            throw new InvalidOperationException($"'{host.Id}' can only initiate a duel while participating.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{host.Id}' requires context.Target (the character to duel) to be set.");

        var opponent = context.Game.Opponent(context.Player);
        if (target.Controller != opponent)
            throw new InvalidOperationException($"'{target.Id}' must be controlled by the opponent.");

        if (!IsParticipating(context.Game, target))
            throw new InvalidOperationException($"'{target.Id}' is not participating.");

        var difference = Math.Abs(context.Player.ShowBid - opponent.ShowBid);
        if (difference > 0)
        {
            var lowBidder = context.Player.ShowBid <= opponent.ShowBid ? context.Player : opponent;
            var otherPlayer = lowBidder == context.Player ? opponent : context.Player;

            switch (context.ChosenChoice)
            {
                case "Pay with honor":
                    lowBidder.Honor = Math.Max(0, lowBidder.Honor - difference);
                    otherPlayer.Honor += difference;
                    break;
                case "Pay with cards":
                    var chosen = context.ChosenDiscardCards
                        ?? throw new InvalidOperationException($"'{host.Id}' requires context.ChosenDiscardCards ({difference} card(s) from '{lowBidder.Name}''s hand) to be set.");

                    if (chosen.Count != difference)
                        throw new InvalidOperationException($"'{host.Id}' requires exactly {difference} chosen card(s).");

                    foreach (var card in chosen)
                    {
                        if (card.Controller != lowBidder || card.Location != "hand")
                            throw new InvalidOperationException($"'{card.Id}' is not a legal duel-cost card (must be in '{lowBidder.Name}''s hand).");

                        ZoneMover.MoveTo(card, lowBidder.Discard, "discard");
                    }
                    break;
                default:
                    throw new InvalidOperationException($"'{host.Id}' requires context.ChosenChoice to be 'Pay with honor' or 'Pay with cards' when there's a bid difference to pay.");
            }
        }

        var winner = context.DuelWinner
            ?? throw new InvalidOperationException($"'{host.Id}' requires context.DuelWinner to be set.");
        var loser = winner == host ? target : host;

        context.Target = loser;
        new BowGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
