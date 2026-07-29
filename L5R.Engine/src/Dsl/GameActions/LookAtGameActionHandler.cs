using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki LookAtAction never mutates state - it only pushes a chat message revealing the
/// card's identity to the acting player. This engine has no per-player hidden-information/
/// visibility model to reveal into (it's a synchronous, fully-visible-to-the-test engine),
/// so there is genuinely nothing further to do: the card stays exactly as it was (still
/// facedown, still wherever it is). This is a verified no-op, not a stub - see
/// iuchi-wayfinder's own test, which asserts Facedown is unchanged after Execute.
/// </summary>
public sealed class LookAtGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("lookAt requires context.Target to be set.");
    }
}
