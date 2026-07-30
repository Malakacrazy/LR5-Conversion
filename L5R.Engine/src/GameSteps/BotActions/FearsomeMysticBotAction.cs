using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// Phase B adapter for fearsome-mystic. Needs nothing beyond Source/Player/Game - the
/// script itself computes its whole target set internally (FearsomeMysticRemoveFateFromLowerGloryOpponents.cs).
/// Legal exactly while the mystic is participating; Invoke never needs a caller-chosen
/// target at all.
/// </summary>
public sealed class FearsomeMysticBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(source) || conflict.Defenders.Contains(source));

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source };
        new FearsomeMysticRemoveFateFromLowerGloryOpponents().Execute(context);
    }
}
