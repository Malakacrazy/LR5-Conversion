namespace L5R.Engine.Cards;

/// <summary>
/// Escape valve for cards whose logic doesn't fit card-schema.json's generic vocabulary.
/// A scriptOverride's `handler` names a type implementing this, resolved by reflection
/// at card-load time. What this interface exposes will grow once the state model (task 8)
/// is rich enough for scripted behavior to actually run against it; for now it exists so
/// the loader has something concrete to check `handler` against.
/// </summary>
public interface ICardScript
{
}
