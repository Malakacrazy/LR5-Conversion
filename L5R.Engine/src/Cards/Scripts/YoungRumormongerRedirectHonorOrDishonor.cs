namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for young-rumormonger: when a character would be honored or
/// dishonored, choose another character controlled by the same player to receive it
/// instead. Needs a target cardCondition referencing the triggering event's card
/// (excluded from selection, and used to match controller) plus event-name-dependent
/// replacement action selection, neither modeled by the closed vocabulary. Stubbed until
/// the state model has honor.
/// </summary>
public sealed class YoungRumormongerRedirectHonorOrDishonor : ICardScript
{
}
