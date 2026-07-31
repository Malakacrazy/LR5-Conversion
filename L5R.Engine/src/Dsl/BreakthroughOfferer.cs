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
/// GameLoop.ConflictPhaseStep is the only caller: it checks IsEligible right after resolving
/// each conflict, awaits the policy's DeclareConflict itself if so, and calls Commit once a
/// real declaration comes back before resolving that one too. No while-loop chaining is
/// needed - the script's own "declarationsThisPhase != 1" check naturally refuses a second
/// chain once the bonus conflict itself gets recorded.
///
/// Split into IsEligible/Commit (rather than one method awaiting the policy internally) so
/// this class doesn't need to become an IEnumerator just to make its one policy call
/// pausable - GameLoop already owns that idiom for every other decision point in the same
/// method, so it just awaits DeclareConflict directly between the two calls here.
/// </summary>
public static class BreakthroughOfferer
{
    public static bool IsEligible(GameState game, Player player, out Card? card, out int cost)
    {
        card = player.Hand.FirstOrDefault(c => c.Id == "breakthrough");
        cost = 0;
        if (card is null)
            return false;

        cost = game.EffectiveCost(card, player);
        if (player.Fate < cost)
            return false;

        var finishedConflict = game.ConflictRecord.LastOrDefault();
        if (finishedConflict is null || finishedConflict.AttackingPlayer != player || finishedConflict.Winner != player)
            return false;

        if (finishedConflict.DeclaredProvince is not { Broken: true })
            return false;

        return game.ConflictDeclarationsThisPhase.Count(d => d.Player == player && !d.Passed) == 1;
    }

    /// <summary>Spends the card and executes its script once the caller has a real bonus declaration in hand - never call this if the awaited DeclareConflict came back null.</summary>
    public static void Commit(GameState game, Player player, Card card, int cost)
    {
        player.Fate -= cost;
        var context = new AbilityContext { Game = game, Player = player, Source = card };
        new BreakthroughDeclareSecondConflict().Execute(context);
        ZoneMover.MoveTo(card, player.Discard, "discard");
    }
}
