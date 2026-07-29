using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// vengeful-oathkeeper: after its controller loses a military conflict, put this character
/// into play from hand, joining the conflict on its controller's side ("event.conflict.
/// loser === context.player && event.conflict.conflictType === 'military'", "location:
/// Locations.Hand"). ringteki's PutIntoPlayAction(intoConflict: true) is the exact same
/// operation this engine's own "putIntoConflict" gameAction already performs (move to play
/// area, join the current conflict as attacker/defender based on context.Player's side) -
/// PutIntoConflictGameActionHandler is reused directly rather than adding a separate,
/// functionally identical "putIntoPlay" handler.
/// </summary>
public sealed class VengefulOathkeeperPutIntoPlayOnMilitaryLoss : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var oathkeeper = context.Source;

        if (oathkeeper.Location != "hand")
            throw new InvalidOperationException($"'{oathkeeper.Id}' can only trigger while in hand.");

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{oathkeeper.Id}' requires an active conflict.");

        if (conflict.Loser != context.Player)
            throw new InvalidOperationException($"'{oathkeeper.Id}' can only trigger when its controller loses the conflict.");

        if (conflict.ConflictType != "military")
            throw new InvalidOperationException($"'{oathkeeper.Id}' can only trigger after a military conflict.");

        context.Target = oathkeeper;
        new PutIntoConflictGameActionHandler().Execute(context, null);
    }
}
