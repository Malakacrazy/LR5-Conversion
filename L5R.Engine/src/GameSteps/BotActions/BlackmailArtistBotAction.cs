using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>blackmail-artist: same Winner-after-the-post-resolution-window shape as asako-diplomat, no target needed.</summary>
public sealed class BlackmailArtistBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict
        && conflict.Winner == actingPlayer
        && conflict.ConflictType == "political"
        && (conflict.Attackers.Contains(source) || conflict.Defenders.Contains(source));

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source };
        new BlackmailArtistTakeHonorOnPoliticalWin().Execute(context);
    }
}
