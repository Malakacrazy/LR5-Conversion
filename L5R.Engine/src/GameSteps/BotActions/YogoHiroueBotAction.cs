using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// Only yogo-hiroue's first moment (Execute: while participating, move a character into this
/// conflict) is adopted here - its second moment (ResolveDishonorChoice, gated on
/// conflict.Winner) needs the conflict's outcome, which isn't known until after this window
/// runs, same reason every other Winner-gated script was rejected. No extra "once per
/// conflict" gate is needed: "while participating" can only be true during the mid-conflict
/// window, which ConflictResolver invokes exactly once per conflict, and usedThisWindow caps
/// hiroue itself to one activation within that single call.
/// </summary>
public sealed class YogoHiroueBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        IsParticipating(game, source) && FindTarget(game, source, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, source, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new YogoHiroueMoveThenMayDishonorOnWin().Execute(context);
    }

    private static Card? FindTarget(GameState game, Card source, Player actingPlayer)
    {
        var conflict = game.CurrentConflict!;
        return actingPlayer.PlayArea.FirstOrDefault(c =>
            c.Type == CardType.Character && c != source
            && !conflict.Attackers.Contains(c) && !conflict.Defenders.Contains(c)
            && !game.IsRestrictedFrom(c, actingPlayer == conflict.AttackingPlayer ? "declareAsAttacker" : "declareAsDefender"));
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
