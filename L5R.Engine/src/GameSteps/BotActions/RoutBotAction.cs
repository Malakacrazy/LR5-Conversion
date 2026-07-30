using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for the event card rout - same shape as OutwitBotAction (military/bushi instead of political/courtier), see its own doc comment.</summary>
public sealed class RoutBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        FindTarget(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new RoutSendHomeOutclassedByBushi().Execute(context);
    }

    private static Card? FindTarget(GameState game, Player actingPlayer) =>
        game.Opponent(actingPlayer).PlayArea.FirstOrDefault(candidate =>
            actingPlayer.PlayArea.Any(myCard =>
                myCard.Traits.Contains("bushi") && IsParticipating(game, myCard)
                && game.EffectiveMilitarySkill(myCard) > game.EffectiveMilitarySkill(candidate)));

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
