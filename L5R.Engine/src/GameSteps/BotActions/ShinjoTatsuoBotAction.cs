using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// Phase B adapter for shinjo-tatsuo. Only exercises the simpler of its two shapes - moving
/// itself alone (context.Target left null, the script's own "just move myself" case) - not
/// the optional-ally variant; a trivial bot doesn't need the extra complexity of also
/// picking a second character to commit.
/// </summary>
public sealed class ShinjoTatsuoBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict && !conflict.Attackers.Contains(source) && !conflict.Defenders.Contains(source);

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source };
        new ShinjoTatsuoMoveSelfAndOptionalAllyToConflict().Execute(context);
    }
}
