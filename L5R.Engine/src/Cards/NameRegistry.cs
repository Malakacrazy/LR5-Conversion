namespace L5R.Engine.Cards;

/// <summary>
/// Catalog of valid names for one DSL vocabulary (effects, game actions, or costs).
/// card-schema.md's documented design: an unknown name is a card-load-time error here,
/// not a JSON Schema enum, so the schema and this catalog can't silently drift apart -
/// this is the single source of truth the schema explicitly defers to.
/// This phase validates names only; wiring each name to real C# behavior against the
/// (currently thin) state model from task 8 is future work.
/// </summary>
public sealed class NameRegistry
{
    private readonly HashSet<string> _names;

    public NameRegistry(IEnumerable<string> names) => _names = new HashSet<string>(names, StringComparer.Ordinal);

    public bool IsRegistered(string name) => _names.Contains(name);
}
