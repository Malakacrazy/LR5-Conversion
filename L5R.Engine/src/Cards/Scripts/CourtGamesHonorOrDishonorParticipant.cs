namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for court-games: during a political conflict, either honor a
/// participating character you control or let your opponent dishonor a participating
/// character they control. Needs a two-level select (choose the action, then choose a
/// card for it) the schema's target.choices can't express. Stubbed until the state model
/// supports nested/dependent selection.
/// </summary>
public sealed class CourtGamesHonorOrDishonorParticipant : ICardScript
{
}
