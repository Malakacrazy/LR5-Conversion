using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>ikoma-eiji: Conflict.Loser is settled by the post-resolution window. Doesn't require ikoma-eiji itself to be participating (the script itself never checks this). Targets the first eligible bushi in provinces/discard.</summary>
public sealed class IkomaEijiBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { ConflictType: "political" } conflict
        && conflict.Loser == actingPlayer
        && FindTarget(actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new IkomaEijiPutBushiIntoPlayOnPoliticalLoss().Execute(context);
    }

    private static Card? FindTarget(Player actingPlayer) =>
        actingPlayer.Provinces.Concat(actingPlayer.Discard)
            .FirstOrDefault(c => c.Type == CardType.Character && c.Traits.Contains("bushi") && c.PrintedCost < 4);
}
