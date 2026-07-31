using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps;

/// <summary>
/// The decision points GameLoop needs from whoever is "playing" a side - a trivial bot for
/// now (step 11's literal ask: "duas estrategias triviais"), a real player-facing prompt
/// pipeline later. Every method is a pure decision (no state mutation) so GameLoop/
/// ConflictResolver stay the only things that actually change GameState.
///
/// Every method returns a Task so a real UI-backed policy can pause - the Task doesn't
/// complete until the human answers - while GameLoop/ActionWindowRunner/ConflictResolver
/// yield a StepAwait wrapping it and let Scheduler genuinely suspend instead of blocking a
/// thread. Bot policies just return Task.FromResult(...): an already-completed Task never
/// actually suspends anything, so bot-only games behave exactly as before this existed.
/// </summary>
public interface IBotPolicy
{
    /// <summary>Dynasty/Fate action windows: which legal CardAction to play, or null to pass.</summary>
    Task<CardAction?> ChooseAction(GameState game, Player player);

    /// <summary>Dynasty/Conflict-phase play windows: which card to play from "hand" or "province" (LegalActions.GetLegalPlays), or null to pass.</summary>
    Task<Card?> ChoosePlay(GameState game, Player player, string location);

    /// <summary>Draw phase's honor bid - the raw ShowBid value (ringteki's honor dial, conventionally 1-5).</summary>
    Task<int> ChooseHonorBid(GameState game, Player player);

    /// <summary>Conflict phase priority: declare a conflict, or null to pass this opportunity.</summary>
    Task<ConflictDeclaration?> DeclareConflict(GameState game, Player player);

    /// <summary>Which of the defender's eligible characters (see ConflictResolver.EligibleDefenders) commit to defend - may be empty (an unopposed conflict).</summary>
    Task<IReadOnlyList<Card>> DeclareDefenders(GameState game, Conflict conflict, Player defender);

    /// <summary>
    /// Phase B: a scripted (scriptOverride) action this bot wants to use this decision point,
    /// if any card it controls has one adopted into a ScriptedActionRegistry and currently
    /// legal - a parallel surface to ChooseAction for cards with bespoke script behavior
    /// instead of a plain abilities.actions[] entry. Null if none apply (including for
    /// policies, like AlwaysPassBotPolicy, that never consult a registry at all).
    /// </summary>
    Task<(Card Source, IBotScriptAction Action)?> ChooseScriptedAction(GameState game, Player player);

    /// <summary>
    /// Phase B: this specific card's adopted scripted action, if any - used by EventResolver
    /// for a scriptOverride'd event (outwit, rout, ...), which has no bridged Card.Actions
    /// entry to check instead (its whole effect lives in the script, not in JSON). Null if
    /// not adopted (including for policies, like AlwaysPassBotPolicy, that never consult a
    /// registry at all).
    /// </summary>
    Task<IBotScriptAction?> ResolveEventScript(string cardId);
}
