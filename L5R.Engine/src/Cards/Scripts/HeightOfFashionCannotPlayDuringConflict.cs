using L5R.Engine.Abilities;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// height-of-fashion: cannot be played while a conflict is currently in progress
/// (ringteki DrawCard.canPlay override). No other ability on this card.
/// </summary>
public sealed class HeightOfFashionCannotPlayDuringConflict : ICardScript
{
    public bool CanPlay(AbilityContext context) => context.Game.CurrentConflict is null;
}
