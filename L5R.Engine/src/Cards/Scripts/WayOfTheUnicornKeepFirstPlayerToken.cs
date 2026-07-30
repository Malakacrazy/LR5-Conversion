using L5R.Engine.Abilities;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// way-of-the-unicorn: interrupts and cancels the automatic end-of-round first-player-token
/// pass (GameState.AdvancePhase()'s ActivePlayer flip on the Dynasty rollover - see
/// GameState.FirstPlayerPassCancelled's own doc comment) so its controller keeps the token
/// for another round. Only the player who currently holds the token (ActivePlayer) can keep
/// it - ringteki's own `event.player === context.player.opponent` check is just "the token
/// would leave me", equivalent to "I am the current first player" from this side of the
/// interrupt.
/// </summary>
public sealed class WayOfTheUnicornKeepFirstPlayerToken : ICardScript
{
    public void Execute(AbilityContext context)
    {
        if (context.Player != context.Game.ActivePlayer)
            throw new InvalidOperationException($"'{context.Source.Id}' can only keep the first player token for the player who currently holds it.");

        context.Game.FirstPlayerPassCancelled = true;
    }
}
