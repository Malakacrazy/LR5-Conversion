using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki SwitchConflictTypeAction (a RingAction, not a CardGameAction - no context.Target
/// involved): toggles the current conflict's type between military and political.
/// canAffect requires an active conflict. No "targetConflictType" param support yet - no
/// ported card in the executable set requests a specific type, only the toggle.
/// </summary>
public sealed class SwitchConflictTypeGameActionHandler : IGameActionHandler
{
    public bool CanAffect(AbilityContext context, JsonElement? parameters) =>
        context.Game.CurrentConflict is not null;

    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException("switchConflictType requires an active conflict.");

        conflict.ConflictType = conflict.ConflictType switch
        {
            "military" => "political",
            "political" => "military",
            _ => throw new InvalidOperationException($"switchConflictType requires the conflict's type to already be 'military' or 'political', was '{conflict.ConflictType ?? "null"}'.")
        };
    }
}
