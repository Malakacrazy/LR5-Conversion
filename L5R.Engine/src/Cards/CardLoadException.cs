namespace L5R.Engine.Cards;

/// <summary>Thrown when a card document references something the engine can't resolve. Loud by design.</summary>
public sealed class CardLoadException : Exception
{
    public CardLoadException(string message) : base(message) { }
}
