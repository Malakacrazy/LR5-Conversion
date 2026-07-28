using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>Parses card-schema.json's action-level `phase` string to Phase.</summary>
public static class Phases
{
    public static Phase Parse(string phase) => phase switch
    {
        "dynasty" => Phase.Dynasty,
        "draw" => Phase.Draw,
        "conflict" => Phase.Conflict,
        "fate" => Phase.Fate,
        "regroup" => Phase.Regroup,
        _ => throw new NotSupportedException($"Unknown phase '{phase}'.")
    };
}
