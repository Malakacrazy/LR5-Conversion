namespace L5R.Engine.Logging;

/// <summary>One canonical, immutable record of a game event.</summary>
public sealed record GameLogEntry(int Sequence, string EventName, IReadOnlyDictionary<string, string> Data);
