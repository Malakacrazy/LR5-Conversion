namespace L5R.Engine.Cards;

/// <summary>One effect/gameAction/cost name found while scanning a card's abilities.</summary>
public sealed record CollectedReference(string Kind, string Name);
