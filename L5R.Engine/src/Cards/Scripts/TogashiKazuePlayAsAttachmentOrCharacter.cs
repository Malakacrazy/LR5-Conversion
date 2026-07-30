using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// togashi-kazue: may be played as an attachment instead of a character (Execute, same
/// PlayAsAttachmentGameActionHandler as tattooed-wanderer - see that handler's doc comment
/// for why Card.Type stays untouched), and while attached (AttachedTo != null - the same
/// fact PlayAsAttachmentGameActionHandler sets, standing in for ringteki's own
/// `context.source.type === CardTypes.Attachment` check since Type itself never changes
/// here) and its parent is participating, has a second action: steal a fate from another
/// participating character and place it on its parent. Two genuinely separate actions at
/// two separate moments (play-time vs. in-play), so - same split as
/// YogoHiroueMoveThenMayDishonorOnWin - StealFate is a second plain method beyond ICardScript,
/// invoked by the caller only once togashi-kazue is actually in play as an attachment.
/// RemoveFateGameActionHandler has no "recipient" param (unused by any other ported card), so
/// the fate transfer to the parent is inlined here, capped to however much was actually
/// removed (0 fate targets remove nothing, so the parent must gain nothing either).
/// </summary>
public sealed class TogashiKazuePlayAsAttachmentOrCharacter : ICardScript
{
    public void Execute(AbilityContext context) => new PlayAsAttachmentGameActionHandler().Execute(context, null);

    public void StealFate(AbilityContext context)
    {
        var kazue = context.Source;
        var parent = kazue.AttachedTo
            ?? throw new InvalidOperationException($"'{kazue.Id}' must currently be attached to steal a fate.");

        if (!IsParticipating(context.Game, parent))
            throw new InvalidOperationException($"'{kazue.Id}''s parent must be participating in the conflict.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{kazue.Id}' requires context.Target (the character to steal a fate from) to be set.");

        if (target == parent)
            throw new InvalidOperationException($"'{kazue.Id}' cannot steal a fate from its own parent.");

        if (!IsParticipating(context.Game, target))
            throw new InvalidOperationException($"'{target.Id}' must be participating in the conflict.");

        var fateBefore = target.Fate;
        new RemoveFateGameActionHandler().Execute(context, null);
        parent.Fate += fateBefore - target.Fate;
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
