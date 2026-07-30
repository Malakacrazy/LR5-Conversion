using System.Text.Json;

namespace L5R.Engine.State;

/// <summary>
/// One player's starting deck for GameSetup - already-parsed card JSON, not file paths.
/// Keeps GameSetup (like CardFactory) decoupled from how/where a card's JSON is stored;
/// the caller (a test today, a real deckbuilder/Unity asset pipeline later) resolves ids to
/// documents. DynastyCards/ConflictCards are pre-split by the caller rather than derived
/// from the card's own JSON, since card-schema.json has no dynasty/conflict "side" field yet
/// (no ported card's executable slice has needed one) - see Player.DynastyDeck's own doc
/// comment.
/// </summary>
public sealed record DeckList(
    JsonElement Stronghold,
    JsonElement? Role,
    IReadOnlyList<JsonElement> DynastyCards,
    IReadOnlyList<JsonElement> ConflictCards);
