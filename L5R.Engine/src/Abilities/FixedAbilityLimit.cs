using L5R.Engine.State;

namespace L5R.Engine.Abilities;

/// <summary>ringteki abilitylimit.js FixedAbilityLimit: never resets on its own.</summary>
public sealed class FixedAbilityLimit : IAbilityLimit
{
    private readonly int _max;
    private readonly Dictionary<Player, int> _useCount = new();

    public FixedAbilityLimit(int max) => _max = max;

    public bool IsAtMax(Player player) => _useCount.GetValueOrDefault(player) >= _max;

    public void Increment(Player player) => _useCount[player] = _useCount.GetValueOrDefault(player) + 1;
}
