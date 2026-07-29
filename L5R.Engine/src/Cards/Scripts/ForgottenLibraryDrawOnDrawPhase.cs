using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// forgotten-library: after the draw phase begins, draw 1 card. Needs GameState.
/// CurrentPhase field inspection, a scalar field not covered by the event.card 'isSelf'
/// convention.
/// </summary>
public sealed class ForgottenLibraryDrawOnDrawPhase : ICardScript
{
    public void Execute(AbilityContext context)
    {
        if (context.Game.CurrentPhase != Phase.Draw)
            throw new InvalidOperationException($"'{context.Source.Id}' can only trigger at the start of the draw phase.");

        new DrawGameActionHandler().Execute(context, null);
    }
}
