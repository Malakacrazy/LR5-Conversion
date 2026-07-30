using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for giver-of-gifts. Two inputs: an attachment the bot controls, and a different character the bot controls to move it to - picks the first legal pairing.</summary>
public sealed class GiverOfGiftsBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        FindMove(actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var move = FindMove(actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot move.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = move.Attachment, PlayAttachTarget = move.NewParent };
        new GiverOfGiftsMoveAttachment().Execute(context);
    }

    private static (Card Attachment, Card NewParent)? FindMove(Player actingPlayer)
    {
        var characters = actingPlayer.PlayArea.Where(c => c.Type == CardType.Character).ToList();

        foreach (var attachment in actingPlayer.PlayArea.Where(c => c.Type == CardType.Attachment && c.AttachedTo is not null))
        {
            var newParent = characters.FirstOrDefault(c => c != attachment.AttachedTo);
            if (newParent is not null)
                return (attachment, newParent);
        }

        return null;
    }
}
