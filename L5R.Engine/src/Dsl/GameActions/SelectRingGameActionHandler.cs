using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki SelectRingAction (selectRingPrompt.js): prompts the player to choose a ring,
/// then runs a nested gameAction against it. No ring-selection UI exists, so the caller
/// supplies the choice directly via context.ChosenRingElement (same convention as
/// ChosenTarget/ChosenCostTarget). Only wrapping "switchConflictElement" is supported so
/// far - ringteki's real selectRing can wrap several ring-targeting actions, but no other
/// ported card's executable slice needs them yet.
/// </summary>
public sealed class SelectRingGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var element = context.ChosenRingElement
            ?? throw new InvalidOperationException("selectRing requires a chosen ring element but none was supplied.");

        var nestedName = parameters?.GetProperty("gameAction").GetProperty("name").GetString()
            ?? throw new InvalidOperationException("selectRing requires params.gameAction (the action to run against the chosen ring).");

        if (nestedName != "switchConflictElement")
            throw new NotSupportedException($"selectRing does not yet support wrapping gameAction '{nestedName}'.");

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException("selectRing requires an active conflict.");

        conflict.Elements.Clear();
        conflict.Elements.Add(element);
    }
}
