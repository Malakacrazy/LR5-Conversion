using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// kitsu-spiritcaller: during a conflict, bow this character (the cost) to put a
/// character from its controller's discard pile into play in the conflict; when the
/// conflict ends, it returns to the bottom of its controller's deck via GameState.
/// EndOfConflictReturns - see that field's own doc comment for why this is a plain list
/// rather than a general delayed-effect system.
/// </summary>
public sealed class KitsuSpiritcallerResurrectUntilConflictEnd : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var spiritcaller = context.Source;

        if (context.Game.CurrentConflict is null)
            throw new InvalidOperationException($"'{spiritcaller.Id}' requires an active conflict.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{spiritcaller.Id}' requires context.Target (the character to resurrect) to be set.");

        if (target.Controller != context.Player)
            throw new InvalidOperationException($"'{target.Id}' must be controlled by '{spiritcaller.Id}''s controller.");

        if (target.Type != CardType.Character)
            throw new InvalidOperationException($"'{target.Id}' must be a character.");

        if (target.Location != "discard")
            throw new InvalidOperationException($"'{target.Id}' must be in its controller's discard pile.");

        context.Target = spiritcaller;
        new BowGameActionHandler().Execute(context, null);

        context.Target = target;
        new PutIntoConflictGameActionHandler().Execute(context, null);

        context.Game.EndOfConflictReturns.Add(target);
    }
}
