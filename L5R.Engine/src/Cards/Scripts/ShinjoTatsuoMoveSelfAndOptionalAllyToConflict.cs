using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// shinjo-tatsuo: during a conflict, move this character and (optionally) one other
/// character you control to the conflict. Needs a per-target optional flag within a
/// targets map, which target.mode's upTo/exactly (single-target cardinality) doesn't
/// express - matches this card's own scriptOverride reason. context.Target carries the
/// optional ally (null for "just move myself"), read before it's overwritten to drive
/// each individual moveToConflict call.
/// </summary>
public sealed class ShinjoTatsuoMoveSelfAndOptionalAllyToConflict : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var tatsuo = context.Source;
        var ally = context.Target;

        if (ally is not null)
        {
            if (ally == tatsuo)
                throw new InvalidOperationException($"'{ally.Id}' must be a character other than '{tatsuo.Id}'.");

            if (ally.Controller != context.Player)
                throw new InvalidOperationException($"'{ally.Id}' must be controlled by '{tatsuo.Id}''s controller.");
        }

        context.Target = tatsuo;
        new MoveToConflictGameActionHandler().Execute(context, null);

        if (ally is not null)
        {
            context.Target = ally;
            new MoveToConflictGameActionHandler().Execute(context, null);
        }
    }
}
