using L5R.Engine.Abilities;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// good-omen: can only be played while the controller has composure - their honor bid
/// (Player.ShowBid) is lower than their opponent's (ringteki DrawCard.canPlay override).
/// The card's targeted action is expressed generically in its JSON.
/// </summary>
public sealed class GoodOmenCannotPlayWithoutComposure : ICardScript
{
    public bool CanPlay(AbilityContext context) =>
        context.Player.ShowBid < context.Game.Opponent(context.Player).ShowBid;
}
