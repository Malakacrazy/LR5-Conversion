using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for i-am-ready. Its input is context.CostTarget, not context.Target - the same field every removeFate-costed card's cost handler would populate. Target pool is any of the bot's own bowed Unicorn characters with fate to spend.</summary>
public sealed class IAmReadyBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        FindTarget(actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, CostTarget = target };
        new IAmReadyReadyTheRemoveFateCostTarget().Execute(context);
    }

    private static Card? FindTarget(Player actingPlayer) =>
        actingPlayer.PlayArea.FirstOrDefault(c => c.Bowed && c.Faction == "unicorn" && c.Fate > 0);
}
