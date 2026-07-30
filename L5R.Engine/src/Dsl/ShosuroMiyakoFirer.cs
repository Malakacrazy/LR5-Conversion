using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// shosuro-miyako: after its controller plays a character from hand, the opponent discards
/// a card at random or dishonors a character they control. Fires directly inside
/// PlayCardGameActionHandler.Execute, the single choke point every card play already goes
/// through (same site WatchCommanderFirer hooks) - the caller there captures whether the
/// played card came from hand before the move happens, since Card.Location is overwritten by
/// ZoneMover.MoveTo. Unlike watch-commander (a reaction to the OPPONENT playing a card), this
/// scans the playing player's own PlayArea, since the ability triggers off its own
/// controller's play. Prefers dishonoring an opponent's character (no randomness needed);
/// falls back to discarding the opponent's first hand card when they have no character in
/// play, matching AsakoDiplomatBotAction's own "prefer X, fall back to Y" shape.
/// </summary>
public static class ShosuroMiyakoFirer
{
    public static void FireEligibleReactions(GameState game, Player playingPlayer)
    {
        var opponent = game.Opponent(playingPlayer);

        foreach (var miyako in playingPlayer.PlayArea.Where(c => c.Id == "shosuro-miyako" && !game.IsBlanked(c)).ToList())
        {
            var opponentCharacter = opponent.PlayArea.FirstOrDefault(c => c.Type == CardType.Character);

            AbilityContext context;
            if (opponentCharacter is not null)
            {
                context = new AbilityContext { Game = game, Player = playingPlayer, Source = miyako, ChosenChoice = "Dishonor a character", Target = opponentCharacter };
            }
            else
            {
                var handCard = opponent.Hand.FirstOrDefault();
                if (handCard is null)
                    continue;

                context = new AbilityContext { Game = game, Player = playingPlayer, Source = miyako, ChosenChoice = "Discard at random", ChosenDiscardCards = new[] { handCard } };
            }

            new ShosuroMiyakoDiscardOrDishonorOnCharacterPlayed().Execute(context);
        }
    }
}
