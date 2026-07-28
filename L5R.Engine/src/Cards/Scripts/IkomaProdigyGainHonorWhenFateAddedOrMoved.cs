namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for ikoma-prodigy: gain 1 honor after fate is placed on this character,
/// whether by entering play with fate or by a fate-move event. The onMoveFate branch
/// needs event.recipient field inspection, not covered by the event.card 'isSelf'
/// convention. Stubbed until the state model has fate-move events.
/// </summary>
public sealed class IkomaProdigyGainHonorWhenFateAddedOrMoved : ICardScript
{
}
