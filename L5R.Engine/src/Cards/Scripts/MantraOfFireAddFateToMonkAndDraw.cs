namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for mantra-of-fire: after an opponent declares a fire conflict, place
/// 1 fate on a monk character (or one with a monk attachment) and draw 1 card. Needs
/// event.ring/event.conflict field inspection beyond the closed predicate vocabulary.
/// Stubbed until the state model has conflicts and rings.
/// </summary>
public sealed class MantraOfFireAddFateToMonkAndDraw : ICardScript
{
}
