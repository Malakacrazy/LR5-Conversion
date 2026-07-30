using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>deathseeker: Conflict.Loser is settled by the post-resolution window. Targets the first opponent character regardless of its fate - the script itself branches remove-fate vs. discard based on the target's fate.</summary>
public sealed class DeathseekerBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict
        && conflict.Loser == actingPlayer
        && conflict.Attackers.Contains(source)
        && FindTarget(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new DeathseekerRemoveFateOrDiscardOnLoss().Execute(context);
    }

    private static Card? FindTarget(GameState game, Player actingPlayer) =>
        game.Opponent(actingPlayer).PlayArea.FirstOrDefault(c => c.Type == CardType.Character);
}
