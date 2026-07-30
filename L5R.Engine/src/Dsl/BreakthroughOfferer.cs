using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.GameSteps;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// breakthrough: after winning a conflict as attacker by breaking its province, if that was
/// the controller's only conflict declaration this phase, immediately declare a second
/// conflict - bypassing the normal ConflictOpportunitiesPerPlayer cap for that round. Its own
/// script only sets up a throwaway placeholder Conflict and appends to
/// ConflictDeclarationsThisPhase (the actual attacker/province/ring for the bonus conflict
/// still needs a real declaration, which ConflictResolver.Resolve builds its own Conflict
/// for anyway, discarding the placeholder) - so this asks the policy for a real
/// ConflictDeclaration *before* spending the card, both to avoid wasting it when no legal
/// second attack exists and to avoid leaving CurrentConflict stuck on the placeholder if
/// there's nothing to declare.
///
/// GameLoop.ConflictPhaseStep is the only caller: it checks this right after resolving each
/// conflict, and if it returns a declaration, resolves that one too before moving on. No
/// while-loop chaining is needed - the script's own "declarationsThisPhase != 1" check
/// naturally refuses a second chain once the bonus conflict itself gets recorded.
/// </summary>
public static class BreakthroughOfferer
{
    public static ConflictDeclaration? TryDeclareBonusConflict(GameState game, Player player, IBotPolicy policy)
    {
        var card = player.Hand.FirstOrDefault(c => c.Id == "breakthrough");
        if (card is null)
            return null;

        var cost = game.EffectiveCost(card, player);
        if (player.Fate < cost)
            return null;

        var finishedConflict = game.ConflictRecord.LastOrDefault();
        if (finishedConflict is null || finishedConflict.AttackingPlayer != player || finishedConflict.Winner != player)
            return null;

        if (finishedConflict.DeclaredProvince is not { Broken: true })
            return null;

        if (game.ConflictDeclarationsThisPhase.Count(d => d.Player == player && !d.Passed) != 1)
            return null;

        var bonusDeclaration = policy.DeclareConflict(game, player);
        if (bonusDeclaration is null)
            return null;

        player.Fate -= cost;
        var context = new AbilityContext { Game = game, Player = player, Source = card };
        new BreakthroughDeclareSecondConflict().Execute(context);
        ZoneMover.MoveTo(card, player.Discard, "discard");

        return bonusDeclaration;
    }
}
