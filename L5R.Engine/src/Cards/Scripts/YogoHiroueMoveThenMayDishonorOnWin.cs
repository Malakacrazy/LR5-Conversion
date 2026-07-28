namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for yogo-hiroue: while participating, move a non-participating
/// character to this conflict; if you win, you may dishonor it. Needs a sequential
/// combinator, a delayedEffect with its own nested when, and a menuPrompt with a
/// conditional target, none modeled by the closed vocabulary. Stubbed until the state
/// model has conflicts.
/// </summary>
public sealed class YogoHiroueMoveThenMayDishonorOnWin : ICardScript
{
}
