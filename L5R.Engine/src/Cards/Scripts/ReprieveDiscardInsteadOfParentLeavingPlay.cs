using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// reprieve: when the attached character would leave play, discard this attachment
/// instead. Ringteki's wouldInterrupt-plus-replacementGameAction shape needs no new
/// mechanism here - this engine has no ability-resolution scheduler at all, so "cancel the
/// original event and replace it" is just "the caller calls this script instead of
/// whatever would have discarded/removed the parent" (the same trust-the-caller
/// convention as every other reactive script). DiscardFromPlayGameActionHandler is reused
/// completely untouched, targeting the attachment itself (ringteki's own default target
/// for a card action is context.source) rather than the parent.
/// </summary>
public sealed class ReprieveDiscardInsteadOfParentLeavingPlay : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var reprieve = context.Source;

        _ = reprieve.AttachedTo
            ?? throw new InvalidOperationException($"'{reprieve.Id}' is not currently attached to anything.");

        context.Target = reprieve;
        new DiscardFromPlayGameActionHandler().Execute(context, null);
    }
}
