using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>display-of-power: needs Conflict.Loser/Unopposed, only settled by the post-resolution window. Runs after ConflictResolver's own ring claim, re-assigning it to this card's controller instead.</summary>
public sealed class DisplayOfPowerBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict
        && conflict.Loser == actingPlayer
        && conflict.Unopposed
        && FindRing(game, conflict) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var ring = FindRing(game, game.CurrentConflict!)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot ring.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, TargetRing = ring };
        new DisplayOfPowerCancelAndClaimRing().Execute(context);
    }

    private static Ring? FindRing(GameState game, Conflict conflict) =>
        conflict.Elements.Count > 0 ? game.Rings.Find(r => r.Element == conflict.Elements[0]) : null;
}
