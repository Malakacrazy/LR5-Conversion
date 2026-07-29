using System.Text.Json;

namespace L5R.Engine.State;

/// <summary>
/// One active reduceNextPlayedCardCost effect (city-of-lies). AppliesTo is a predicate
/// evaluated against whichever card is being considered for GameState.EffectiveCost -
/// null means it applies to any card. No "consume after one use" semantics - there's no
/// play-a-card event in this engine to trigger consumption on, so a reduction simply lasts
/// as long as its stated Duration, same expiry as everything else.
/// </summary>
public sealed class PlayerCostReduction
{
    public required Player Player { get; init; }
    public required int Amount { get; init; }
    public JsonElement? AppliesTo { get; init; }
    public required string Duration { get; init; }
}
