using L5R.Engine.Cards;
using L5R.Engine.Logging;
using L5R.Engine.Randomness;

namespace L5R.Engine.State;

/// <summary>
/// Ports ringteki's SetupPhase (setupphase.js) minus its interactive steps - no province-
/// ordering or mulligan prompts exist yet, so provinces are dealt in deck order and starting
/// hands are kept as drawn (a deliberate v1 simplification for the headless bot harness, not
/// a gap - a real client/UI would insert those choices here later). Builds both players'
/// decks via CardFactory, shuffles with the supplied SeededRandom (previously unused - see
/// its own doc comment), deals 4 facedown provinces, draws a 4-card starting hand, and sets
/// starting honor from the stronghold (GameState.SetHonorFromStronghold).
/// </summary>
public static class GameSetup
{
    private const int ProvinceCount = 4;
    private const int StartingHandSize = 4;

    /// <summary>eventLog is optional (defaults to null, logging nothing) so M2's own tests, written before EventLog was wired in, don't need updating for a milestone that isn't theirs.</summary>
    public static void SetUpGame(GameState game, DeckList player1Deck, DeckList player2Deck, SeededRandom rng, EventLog? eventLog = null)
    {
        SetUpPlayer(game, game.Player1, player1Deck, rng, eventLog);
        SetUpPlayer(game, game.Player2, player2Deck, rng, eventLog);
    }

    private static void SetUpPlayer(GameState game, Player player, DeckList deck, SeededRandom rng, EventLog? eventLog)
    {
        player.Stronghold = CardFactory.BuildCard(deck.Stronghold, player);
        if (deck.Role is { } role)
            player.Role = CardFactory.BuildCard(role, player);

        player.DynastyDeck.AddRange(deck.DynastyCards.Select(card => CardFactory.BuildCard(card, player)));
        rng.Shuffle(player.DynastyDeck);
        eventLog?.Record("deckShuffled", new Dictionary<string, string> { ["player"] = player.Name, ["deck"] = "dynasty" });

        player.Deck.AddRange(deck.ConflictCards.Select(card => CardFactory.BuildCard(card, player)));
        rng.Shuffle(player.Deck);
        eventLog?.Record("deckShuffled", new Dictionary<string, string> { ["player"] = player.Name, ["deck"] = "conflict" });

        DealProvinces(player);
        DrawStartingHand(player);

        game.SetHonorFromStronghold(player);
    }

    private static void DealProvinces(Player player)
    {
        for (var i = 0; i < ProvinceCount && player.DynastyDeck.Count > 0; i++)
        {
            var card = player.DynastyDeck[0];
            player.DynastyDeck.RemoveAt(0);
            card.Facedown = true;
            card.Location = "province";
            card.ProvinceSlot = i.ToString();
            player.Provinces.Add(card);
        }
    }

    private static void DrawStartingHand(Player player)
    {
        for (var i = 0; i < StartingHandSize && player.Deck.Count > 0; i++)
        {
            var card = player.Deck[0];
            player.Deck.RemoveAt(0);
            card.Location = "hand";
            player.Hand.Add(card);
        }
    }
}
