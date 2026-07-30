using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// Phase B adapter for the event card strength-in-numbers - same "needs an active conflict"
/// shape as outwit/rout, unblocked by the mid-conflict action window. Target pool is the
/// current conflict's defenders whose effective glory is at most the number of attackers.
/// </summary>
public sealed class StrengthInNumbersBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        FindTarget(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new StrengthInNumbersSendHomeLowGloryDefender().Execute(context);
    }

    private static Card? FindTarget(GameState game, Player actingPlayer)
    {
        var conflict = game.CurrentConflict;
        if (conflict is null || conflict.AttackingPlayer != actingPlayer)
            return null;

        return conflict.Defenders.FirstOrDefault(d => game.EffectiveGlory(d) <= conflict.Attackers.Count);
    }
}
