using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki CancelAction: `event.context.cancel()`. Our engine has no interrupt-window
/// scheduler to automatically hand this ability a reference to the event it's interrupting,
/// so the caller sets context.InterruptedAbility directly (see its own doc comment) before
/// running this - the same convention as ChosenRingElement/ChosenBidDirection.
/// No "replacementGameAction" support (ringteki lets a cancel substitute a different effect
/// for the cancelled one) - no ported card in the executable set needs it.
/// </summary>
public sealed class CancelGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var pending = context.InterruptedAbility
            ?? throw new InvalidOperationException("cancel requires context.InterruptedAbility to be set.");

        pending.Cancel();
    }
}
