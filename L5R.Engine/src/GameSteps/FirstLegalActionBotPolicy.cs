using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps;

/// <summary>
/// Step 11's other trivial strategy: always takes the first legal option at every decision
/// point - the first legal CardAction, the first unclaimed/uncontested ring paired with the
/// first attackable province and a single attacker, and the first eligible defender (if
/// any). Bids 1 (the honor dial's minimum meaningful value) every Draw phase.
/// </summary>
public sealed class FirstLegalActionBotPolicy : IBotPolicy
{
    public CardAction? ChooseAction(GameState game, Player player) =>
        LegalActions.GetLegalActions(game, player).FirstOrDefault();

    public Card? ChoosePlay(GameState game, Player player, string location) =>
        LegalActions.GetLegalPlays(game, player, location).FirstOrDefault();

    public int ChooseHonorBid(GameState game, Player player) => 1;

    public ConflictDeclaration? DeclareConflict(GameState game, Player player)
    {
        var attackers = ConflictResolver.EligibleAttackers(game, player);
        if (attackers.Count == 0)
            return null;

        var provinces = ConflictResolver.AttackableProvinces(game.Opponent(player));
        if (provinces.Count == 0)
            return null;

        var ring = game.Rings.FirstOrDefault(r => !r.Claimed && !r.Contested);
        if (ring is null)
            return null;

        return new ConflictDeclaration(ring.Element, provinces[0], new[] { attackers[0] });
    }

    public IReadOnlyList<Card> DeclareDefenders(GameState game, Conflict conflict, Player defender)
    {
        var eligible = ConflictResolver.EligibleDefenders(game, defender);
        return eligible.Count > 0 ? new[] { eligible[0] } : Array.Empty<Card>();
    }
}
