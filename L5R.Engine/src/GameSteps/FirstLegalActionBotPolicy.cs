using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps;

/// <summary>
/// Step 11's other trivial strategy: always takes the first legal option at every decision
/// point - the first legal CardAction, the first unclaimed/uncontested ring paired with the
/// first attackable province and a single attacker, and the first eligible defender (if
/// any). Bids 1 (the honor dial's minimum meaningful value) every Draw phase.
///
/// scriptedActions is optional (default null, meaning "no Phase B scripted actions
/// available") so every existing caller that predates Phase B keeps working unchanged.
/// </summary>
public sealed class FirstLegalActionBotPolicy : IBotPolicy
{
    private readonly ScriptedActionRegistry? _scriptedActions;

    public FirstLegalActionBotPolicy(ScriptedActionRegistry? scriptedActions = null) => _scriptedActions = scriptedActions;

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

    public (Card Source, IBotScriptAction Action)? ChooseScriptedAction(GameState game, Player player)
    {
        if (_scriptedActions is null)
            return null;

        // Event-typed scripted actions (outwit, rout, ...) only ever fire as part of playing
        // the card (EventResolver.ResolveAndDiscard, via ResolveEventScript) - never as a
        // free-standing "activate this card sitting in hand/play" choice the way a
        // character/holding's own repeatable scripted ability works.
        foreach (var card in game.AllCards().Where(c => c.Controller == player && c.Type != CardType.Event))
        {
            var action = _scriptedActions.Resolve(card.Id);
            if (action is not null && action.IsLegal(game, card, player))
                return (card, action);
        }

        // GameState.AllCards() deliberately excludes Player.Stronghold (Hand+PlayArea only) -
        // mountain-s-anvil-castle is the one ported card with an activatable scripted ability
        // on the stronghold itself, so it needs one extra, explicit check here rather than
        // widening AllCards() and risking exposing strongholds to unrelated board-wide scans.
        if (player.Stronghold is { } stronghold)
        {
            var strongholdAction = _scriptedActions.Resolve(stronghold.Id);
            if (strongholdAction is not null && strongholdAction.IsLegal(game, stronghold, player))
                return (stronghold, strongholdAction);
        }

        return null;
    }

    public IBotScriptAction? ResolveEventScript(string cardId) => _scriptedActions?.Resolve(cardId);
}
