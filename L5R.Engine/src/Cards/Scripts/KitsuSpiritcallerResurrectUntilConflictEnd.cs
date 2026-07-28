namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for kitsu-spiritcaller: during a conflict, bow this character and put a
/// character from a discard pile into play in the conflict; if it's still in play when
/// the conflict ends, return it to the bottom of its deck. Needs a 'then' chained
/// follow-up with a delayedEffect keyed on onConflictFinished, neither modeled by the
/// schema. Stubbed until the state model has conflicts.
/// </summary>
public sealed class KitsuSpiritcallerResurrectUntilConflictEnd : ICardScript
{
}
