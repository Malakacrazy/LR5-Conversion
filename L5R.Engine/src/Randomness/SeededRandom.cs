namespace L5R.Engine.Randomness;

/// <summary>
/// Per-match RNG. Implements splitmix64 directly rather than wrapping System.Random:
/// System.Random's algorithm is a BCL implementation detail, not a spec, and isn't
/// guaranteed to match between .NET and Mono/IL2CPP - which the Unity client will
/// eventually run on. Since replay/spectator/reconnection require the client and the
/// authoritative server to reproduce the same match from the same seed, the RNG needs to
/// be identical on every runtime by construction. splitmix64 is pure integer arithmetic:
/// the same on every platform, no BCL involved.
/// </summary>
public sealed class SeededRandom
{
    private ulong _state;

    public SeededRandom(ulong seed) => _state = seed;

    public ulong NextUInt64()
    {
        _state += 0x9E3779B97F4A7C15UL;
        ulong z = _state;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    /// <summary>
    /// Uniform integer in [0, exclusiveMax). Uses modulo rather than rejection sampling -
    /// the resulting bias is on the order of exclusiveMax / 2^64, unmeasurable for
    /// anything this game rolls (deck sizes, dice-like values).
    /// </summary>
    public int Next(int exclusiveMax)
    {
        if (exclusiveMax <= 0)
            throw new ArgumentOutOfRangeException(nameof(exclusiveMax));

        return (int)(NextUInt64() % (ulong)exclusiveMax);
    }

    /// <summary>Fisher-Yates shuffle, in place.</summary>
    public void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
