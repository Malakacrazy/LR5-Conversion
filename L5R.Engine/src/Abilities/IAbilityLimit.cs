using L5R.Engine.State;

namespace L5R.Engine.Abilities;

/// <summary>ringteki abilitylimit.js.</summary>
public interface IAbilityLimit
{
    bool IsAtMax(Player player);
    void Increment(Player player);
}
