using L5R.Engine.Abilities;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// blackmail: can only be played while the controller is less honorable than their
/// opponent (ringteki DrawCard.canPlay override). The card's targeted action is expressed
/// generically in its JSON; ringteki's own anotherUniqueInPlay cardCondition edge case is
/// intentionally dropped (see the card's own scriptOverride reason).
/// </summary>
public sealed class BlackmailCannotPlayUnlessLessHonorable : ICardScript
{
    public bool CanPlay(AbilityContext context) =>
        context.Player.Honor < context.Game.Opponent(context.Player).Honor;
}
