using L5R.Engine.State;

namespace L5R.Engine.Abilities;

/// <summary>Default limit for actions that don't specify one: never at max.</summary>
public sealed class UnlimitedAbilityLimit : IAbilityLimit
{
    public static readonly UnlimitedAbilityLimit Instance = new();

    private UnlimitedAbilityLimit() { }

    public bool IsAtMax(Player player) => false;

    public void Increment(Player player) { }
}
