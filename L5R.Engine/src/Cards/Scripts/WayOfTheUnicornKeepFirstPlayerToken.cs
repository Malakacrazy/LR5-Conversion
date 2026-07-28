namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for way-of-the-unicorn: when you would pass the first player token,
/// keep it instead. Needs event.player field inspection beyond the closed predicate
/// vocabulary. Stubbed until the state model has a first-player token.
/// </summary>
public sealed class WayOfTheUnicornKeepFirstPlayerToken : ICardScript
{
}
