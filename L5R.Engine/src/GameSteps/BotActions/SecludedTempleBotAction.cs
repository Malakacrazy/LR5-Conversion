using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// Phase B adapter for secluded-temple. Its real trigger ("the conflict phase begins")
/// lines up precisely with the pre-conflict action window's own timing; it can also re-fire
/// during the mid-conflict window within the same phase if still outnumbered - more generous
/// than "once per phase", but each firing consumes a real, finite opponent resource (fate),
/// so it self-limits rather than looping unboundedly.
/// </summary>
public sealed class SecludedTempleBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentPhase == Phase.Conflict
        && actingPlayer.PlayArea.Count < game.Opponent(actingPlayer).PlayArea.Count
        && FindTarget(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new SecludedTempleRemoveFateWhenOutnumbered().Execute(context);
    }

    private static Card? FindTarget(GameState game, Player actingPlayer) =>
        game.Opponent(actingPlayer).PlayArea.FirstOrDefault(c => c.Type == CardType.Character);
}
