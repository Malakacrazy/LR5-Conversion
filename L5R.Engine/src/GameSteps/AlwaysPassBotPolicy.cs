using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps;

/// <summary>One of step 11's two trivial strategies: never acts, never attacks, never defends. Bids 0 (the honor dial's minimum) every Draw phase.</summary>
public sealed class AlwaysPassBotPolicy : IBotPolicy
{
    public Task<CardAction?> ChooseAction(GameState game, Player player) => Task.FromResult<CardAction?>(null);

    public Task<Card?> ChoosePlay(GameState game, Player player, string location) => Task.FromResult<Card?>(null);

    public Task<int> ChooseHonorBid(GameState game, Player player) => Task.FromResult(0);

    public Task<ConflictDeclaration?> DeclareConflict(GameState game, Player player) => Task.FromResult<ConflictDeclaration?>(null);

    public Task<IReadOnlyList<Card>> DeclareDefenders(GameState game, Conflict conflict, Player defender) => Task.FromResult<IReadOnlyList<Card>>(Array.Empty<Card>());

    public Task<(Card Source, IBotScriptAction Action)?> ChooseScriptedAction(GameState game, Player player) => Task.FromResult<(Card Source, IBotScriptAction Action)?>(null);

    public Task<IBotScriptAction?> ResolveEventScript(string cardId) => Task.FromResult<IBotScriptAction?>(null);
}
