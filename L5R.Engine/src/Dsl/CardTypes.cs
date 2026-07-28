using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>Parses card-schema.json's `type`/`cardType` string values to CardType.</summary>
public static class CardTypes
{
    public static CardType Parse(string cardType) => cardType switch
    {
        "character" => CardType.Character,
        "holding" => CardType.Holding,
        "event" => CardType.Event,
        "attachment" => CardType.Attachment,
        "province" => CardType.Province,
        "stronghold" => CardType.Stronghold,
        "role" => CardType.Role,
        _ => throw new NotSupportedException($"Unknown cardType '{cardType}'.")
    };
}
