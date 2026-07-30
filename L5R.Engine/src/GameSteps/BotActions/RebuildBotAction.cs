using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for rebuild. Two inputs: an unbroken province card of the bot's own (the cost) and a holding from the bot's own discard pile (the target) - picks the first legal pairing.</summary>
public sealed class RebuildBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        FindMove(actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var move = FindMove(actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot move.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, CostTarget = move.Province, Target = move.Holding };
        new RebuildReplaceProvinceCardWithHolding().Execute(context);
    }

    private static (Card Province, Card Holding)? FindMove(Player actingPlayer)
    {
        var province = actingPlayer.Provinces.FirstOrDefault(c => !c.Broken && c.ProvinceSlot is not null);
        var holding = actingPlayer.Discard.FirstOrDefault(c => c.Type == CardType.Holding);

        return province is not null && holding is not null ? (province, holding) : null;
    }
}
