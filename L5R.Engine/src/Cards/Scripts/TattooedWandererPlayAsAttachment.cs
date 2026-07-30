using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for tattooed-wanderer: may be played as an attachment instead of a
/// character. See PlayAsAttachmentGameActionHandler for why this doesn't need Card.Type to
/// become mutable - the card's printed Type stays Character even when played this way; only
/// AttachedTo (already the sole thing whileAttached/attach-restriction logic keys off) is
/// set. Its own in-play effect (whileAttached "addKeyword": "covert") is already generic
/// JSON DSL, unaffected by this alternate play mode.
/// </summary>
public sealed class TattooedWandererPlayAsAttachment : ICardScript
{
    public void Execute(AbilityContext context) => new PlayAsAttachmentGameActionHandler().Execute(context, null);
}
