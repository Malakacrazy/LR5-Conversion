using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// mountain-s-anvil-castle: bow this stronghold during a conflict to buff a participant with
/// attachments. The one ported card with an activatable scripted ability on a Stronghold
/// rather than a Hand/PlayArea card - see FirstLegalActionBotPolicy.ChooseScriptedAction's
/// own doc comment for the small, explicit extension that makes this discoverable at all.
/// Targets a participant the bot controls that already has at least one attachment.
/// </summary>
public sealed class MountainsAnvilCastleBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is not null && !source.Bowed && FindTarget(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot target.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target };
        new MountainsAnvilCastleBonusForAttachments().Execute(context);
    }

    private static Card? FindTarget(GameState game, Player actingPlayer)
    {
        var conflict = game.CurrentConflict;
        if (conflict is null) return null;

        return conflict.Attackers.Concat(conflict.Defenders)
            .FirstOrDefault(c => c.Controller == actingPlayer && game.AllCards().Any(a => a.AttachedTo == c));
    }
}
