using System.Linq;
using System.Text;
using System.Text.Json;

namespace L5R.Engine.Logging;

/// <summary>
/// Canonical, serializable record of every relevant game event - entering play, phase
/// changes, ability resolution, target choices, etc. This is the prerequisite for
/// replay, spectating, and reconnection: two matches driven by the same command stream
/// from the same seed must produce byte-identical logs.
/// </summary>
public sealed class EventLog
{
    private static readonly IReadOnlyDictionary<string, string> EmptyData = new Dictionary<string, string>();

    private readonly List<GameLogEntry> _entries = new();

    public IReadOnlyList<GameLogEntry> Entries => _entries;

    public void Record(string eventName, IReadOnlyDictionary<string, string>? data = null)
    {
        _entries.Add(new GameLogEntry(_entries.Count, eventName, data ?? EmptyData));
    }

    /// <summary>
    /// Deterministic byte representation: fixed field order, and entry data keys sorted
    /// ordinally, so two logs built from identical game state produce byte-identical
    /// output regardless of Dictionary iteration order (an implementation detail, not a
    /// contract - not something a replay comparison should ever depend on).
    /// </summary>
    public byte[] ToCanonicalBytes()
    {
        var builder = new StringBuilder();
        foreach (var entry in _entries)
        {
            var sortedData = new SortedDictionary<string, string>(
                entry.Data.ToDictionary(kv => kv.Key, kv => kv.Value),
                StringComparer.Ordinal);
            var line = JsonSerializer.Serialize(new
            {
                entry.Sequence,
                entry.EventName,
                Data = sortedData
            });
            builder.Append(line).Append('\n');
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}
