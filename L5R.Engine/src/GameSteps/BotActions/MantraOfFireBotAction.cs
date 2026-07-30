using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>mantra-of-fire: reacts to the opponent declaring a fire conflict, checkable in either in-conflict window. Targets a monk (or monk-attachment-holder) the bot controls.</summary>
public sealed class MantraOfFireBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict
        && conflict.Elements.Contains("fire")
        && conflict.AttackingPlayer == game.Opponent(actingPlayer)
        && FindTarget(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new MantraOfFireAddFateToMonkAndDraw().Execute(context);
    }

    private static Card? FindTarget(GameState game, Player actingPlayer) =>
        actingPlayer.PlayArea.FirstOrDefault(c =>
            c.Traits.Contains("monk") || game.AllCards().Any(a => a.AttachedTo == c && a.Traits.Contains("monk")));
}
