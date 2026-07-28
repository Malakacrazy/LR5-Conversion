namespace L5R.Engine.State;

public sealed class GameState
{
    public required Player Player1 { get; init; }
    public required Player Player2 { get; init; }
    public Phase CurrentPhase { get; set; }
    public required Player ActivePlayer { get; set; }

    /// <summary>All cards controlled by either player, regardless of zone.</summary>
    public IEnumerable<Card> AllCards() => Player1.Hand.Concat(Player1.PlayArea).Concat(Player2.Hand).Concat(Player2.PlayArea);
}
