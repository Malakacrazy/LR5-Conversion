using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>calling-in-favors: no conflict requirement. Needs an own non-dishonored character (the cost) and an opponent's in-play attachment (the target).</summary>
public sealed class CallingInFavorsBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        FindCostTarget(actingPlayer) is not null && FindAttachment(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var costTarget = FindCostTarget(actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot cost target.");
        var attachment = FindAttachment(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot attachment target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, CostTarget = costTarget, Target = attachment };
        new CallingInFavorsAttachOrDiscard().Execute(context);
    }

    private static Card? FindCostTarget(Player actingPlayer) =>
        actingPlayer.PlayArea.FirstOrDefault(c => c.Type == CardType.Character && !c.IsDishonored);

    private static Card? FindAttachment(GameState game, Player actingPlayer) =>
        game.Opponent(actingPlayer).PlayArea.FirstOrDefault(c => c.Type == CardType.Attachment);
}
