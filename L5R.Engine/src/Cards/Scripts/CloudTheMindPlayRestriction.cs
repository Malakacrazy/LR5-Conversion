using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// cloud-the-mind: ringteki DrawCard.canPlay override restricting play to when the
/// controller has a shugenja character in play. The card's whileAttached blank effect is
/// expressed generically in its JSON.
/// </summary>
public sealed class CloudTheMindPlayRestriction : ICardScript
{
    public bool CanPlay(AbilityContext context) =>
        context.Player.PlayArea.Any(c => c.Type == CardType.Character && c.Traits.Contains("shugenja"));
}
