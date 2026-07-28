using L5R.Engine.State;

namespace L5R.Engine.Abilities;

/// <summary>ringteki AbilityContext.ts: the game/player/source triple every check runs against.</summary>
public sealed class AbilityContext
{
    public required GameState Game { get; init; }
    public required Player Player { get; init; }
    public required Card Source { get; init; }
}
