using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Dsl;

/// <summary>
/// Maps a GameActions.ts name to its executable handler. Deliberately small - grows one
/// entry at a time as a real ported card needs the next game action.
/// </summary>
public sealed class GameActionRegistry
{
    private readonly Dictionary<string, IGameActionHandler> _handlers = new()
    {
        ["placeFate"] = new PlaceFateGameActionHandler()
    };

    public IGameActionHandler Resolve(string name) =>
        _handlers.TryGetValue(name, out var handler)
            ? handler
            : throw new NotSupportedException($"No executable handler registered for gameAction '{name}' yet.");
}
