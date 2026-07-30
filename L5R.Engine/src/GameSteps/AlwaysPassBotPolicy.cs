using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps;

/// <summary>One of step 11's two trivial strategies: never acts, never attacks, never defends. Bids 0 (the honor dial's minimum) every Draw phase.</summary>
public sealed class AlwaysPassBotPolicy : IBotPolicy
{
    public CardAction? ChooseAction(GameState game, Player player) => null;

    public Card? ChoosePlay(GameState game, Player player, string location) => null;

    public int ChooseHonorBid(GameState game, Player player) => 0;

    public ConflictDeclaration? DeclareConflict(GameState game, Player player) => null;

    public IReadOnlyList<Card> DeclareDefenders(GameState game, Conflict conflict, Player defender) => Array.Empty<Card>();

    public (Card Source, IBotScriptAction Action)? ChooseScriptedAction(GameState game, Player player) => null;

    public IBotScriptAction? ResolveEventScript(string cardId) => null;
}
