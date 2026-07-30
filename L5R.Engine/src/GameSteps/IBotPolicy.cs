using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps;

/// <summary>
/// The decision points GameLoop needs from whoever is "playing" a side - a trivial bot for
/// now (step 11's literal ask: "duas estrategias triviais"), a real player-facing prompt
/// pipeline later. Every method is a pure decision (no state mutation) so GameLoop/
/// ConflictResolver stay the only things that actually change GameState.
/// </summary>
public interface IBotPolicy
{
    /// <summary>Dynasty/Fate action windows: which legal CardAction to play, or null to pass.</summary>
    CardAction? ChooseAction(GameState game, Player player);

    /// <summary>Dynasty/Conflict-phase play windows: which card to play from "hand" or "province" (LegalActions.GetLegalPlays), or null to pass.</summary>
    Card? ChoosePlay(GameState game, Player player, string location);

    /// <summary>Draw phase's honor bid - the raw ShowBid value (ringteki's honor dial, conventionally 1-5).</summary>
    int ChooseHonorBid(GameState game, Player player);

    /// <summary>Conflict phase priority: declare a conflict, or null to pass this opportunity.</summary>
    ConflictDeclaration? DeclareConflict(GameState game, Player player);

    /// <summary>Which of the defender's eligible characters (see ConflictResolver.EligibleDefenders) commit to defend - may be empty (an unopposed conflict).</summary>
    IReadOnlyList<Card> DeclareDefenders(GameState game, Conflict conflict, Player defender);
}
