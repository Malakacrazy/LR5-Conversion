using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for enlightened-warrior. Ring is derived from the current conflict's own declared element, not chosen freely.</summary>
public sealed class EnlightenedWarriorBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict && conflict.AttackingPlayer == game.Opponent(actingPlayer)
        && FindRing(game, conflict) is { Fate: > 0 };

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var conflict = game.CurrentConflict!;
        var ring = FindRing(game, conflict)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot ring.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, TargetRing = ring };
        new EnlightenedWarriorGainFateOnOpponentRingSelect().Execute(context);
    }

    private static Ring? FindRing(GameState game, Conflict conflict) =>
        conflict.Elements.Count > 0 ? game.Rings.FirstOrDefault(r => r.Element == conflict.Elements[0]) : null;
}
