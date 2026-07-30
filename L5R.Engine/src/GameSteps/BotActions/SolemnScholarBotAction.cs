using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for solemn-scholar. Legal target pool is exactly the current conflict's attackers; picks the first one.</summary>
public sealed class SolemnScholarBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.Rings.Single(r => r.Element == "earth").ClaimedBy == actingPlayer
        && game.CurrentConflict?.Attackers.Count > 0;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = game.CurrentConflict!.Attackers[0];
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new SolemnScholarBowAttackerIfEarthClaimed().Execute(context);
    }
}
