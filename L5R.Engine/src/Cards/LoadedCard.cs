namespace L5R.Engine.Cards;

public sealed record ScriptOverrideInfo(string Reason, Type HandlerType);

public sealed record LoadedCard(
    string Id,
    string Name,
    string Type,
    IReadOnlyList<CollectedReference> References,
    ScriptOverrideInfo? ScriptOverride);
