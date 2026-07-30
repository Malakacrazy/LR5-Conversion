using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// way-of-the-unicorn: interrupts and cancels the automatic end-of-round first-player-token
/// pass (GameState.AdvancePhase()'s ActivePlayer flip on the Dynasty rollover - see
/// GameState.FirstPlayerPassCancelled's own doc comment) so its controller keeps the token
/// for another round. Only the player who currently holds the token (ActivePlayer) can keep
/// it - ringteki's own `event.player === context.player.opponent` check is just "the token
/// would leave me", equivalent to "I am the current first player" from this side of the
/// interrupt.
///
/// CanPlay restricts this to the Fate phase (the only moment the pass it cancels can even
/// happen) - added specifically so LegalActions.GetLegalPlays never offers it during the
/// Dynasty/Draw/Conflict phases' own generic hand-play windows, where a bot would otherwise
/// play it uselessly (no bridged Card.Actions entry and no ScriptedActionRegistry
/// registration means it would just discard with no effect - see WayOfTheUnicornOfferer,
/// the one place its effect can actually happen).
/// </summary>
public sealed class WayOfTheUnicornKeepFirstPlayerToken : ICardScript
{
    public bool CanPlay(AbilityContext context) => context.Game.CurrentPhase == Phase.Fate;

    public void Execute(AbilityContext context)
    {
        if (context.Player != context.Game.ActivePlayer)
            throw new InvalidOperationException($"'{context.Source.Id}' can only keep the first player token for the player who currently holds it.");

        context.Game.FirstPlayerPassCancelled = true;
    }
}
