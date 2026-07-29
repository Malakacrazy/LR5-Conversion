using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki PutIntoPlayAction(intoConflict: true) (GameActions.ts putIntoConflict): moves
/// context.Target into its controller's play area and adds it to the current conflict.
/// Unlike moveToConflict (which joins based on the target card's own controller),
/// putIntoConflict joins based on context.Player - the ability's controller, not the
/// card's - since shosuro-actress puts an *opponent's* discarded character into play on
/// *her own* side of the conflict. canAffect requires an active conflict and that the card
/// isn't already in play; the real canAffect's unique-in-play/participation-restriction/
/// conflict-type-dash checks aren't modeled yet (no ported card's executable slice needs
/// them - matches the trust-the-caller convention used for chosenTarget elsewhere).
/// </summary>
public sealed class PutIntoConflictGameActionHandler : IGameActionHandler
{
    public bool CanAffect(AbilityContext context, JsonElement? parameters) =>
        context.Game.CurrentConflict is not null && context.Target is { Location: not "play area" };

    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var target = context.Target
            ?? throw new InvalidOperationException("putIntoConflict requires context.Target to be set.");

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException("putIntoConflict requires an active conflict.");

        ZoneMover.MoveTo(target, target.Controller.PlayArea, "play area");

        if (context.Player == conflict.AttackingPlayer)
            conflict.Attackers.Add(target);
        else
            conflict.Defenders.Add(target);
    }
}
