using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// ikoma-prodigy: gain 1 honor after fate is placed on this character, whether by entering
/// play with fate or by a fate-move event (both of ringteki's two "when" branches collapse
/// to the same observable post-condition: this character currently has fate on it that it
/// didn't have before the triggering event - checked here as simply "has fate right now",
/// since there's no event bus to distinguish "just arrived" from "already had it"). Reuses
/// GainHonorGameActionHandler directly (already targets context.Player).
/// </summary>
public sealed class IkomaProdigyGainHonorWhenFateAddedOrMoved : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var prodigy = context.Source;

        if (prodigy.Fate <= 0)
            throw new InvalidOperationException($"'{prodigy.Id}' requires fate on itself to trigger.");

        new GainHonorGameActionHandler().Execute(context, null);
    }
}
