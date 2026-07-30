using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// honored-blade: Conflict.Winner is settled by the post-resolution window. The script
/// credits honor to context.Player, which it expects to be the attached character's
/// controller (not necessarily the attachment's own controller) - set explicitly here rather
/// than passing through actingPlayer (the scanned card's own controller), matching the
/// script's own doc comment.
/// </summary>
public sealed class HonoredBladeBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        source.AttachedTo is { } parent
        && game.CurrentConflict is { } conflict
        && conflict.Winner == parent.Controller
        && (conflict.Attackers.Contains(parent) || conflict.Defenders.Contains(parent));

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var parent = source.AttachedTo
            ?? throw new InvalidOperationException($"'{source.Id}' is not currently attached to anything.");

        var context = new AbilityContext { Game = game, Player = parent.Controller, Source = source };
        new HonoredBladeGainHonorWhenParentWins().Execute(context);
    }
}
