using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for borderlands-fortifications. Legal target pool is any other of its controller's own province cards; picks the first one.</summary>
public sealed class BorderlandsFortificationsBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        actingPlayer.Provinces.Contains(source) && actingPlayer.Provinces.Any(c => c != source);

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = actingPlayer.Provinces.First(c => c != source);
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new BorderlandsFortificationsSwapWithProvinceCard().Execute(context);
    }
}
