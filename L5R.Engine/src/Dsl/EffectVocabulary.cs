using System.Text.Json;

namespace L5R.Engine.Dsl;

/// <summary>
/// Shared mapping from an effect's {name, value} shape to what it actually does - used by
/// both CardLastingEffectGameActionHandler (a directly-invoked, one-shot application, where
/// an unrecognized effect must throw per the fail-loud policy) and GameState's persistent-
/// effect scan (a passive background query touching every persistent effect on the board,
/// most of which are irrelevant to whichever specific stat/restriction is being asked
/// about - an unrecognized effect there is simply not applicable, not a bug, so the Try*
/// methods return false instead of throwing).
/// </summary>
public static class EffectVocabulary
{
    public static bool TryGetStatDeltas(string? effectName, int value, out IReadOnlyList<(string Stat, int Value)> deltas)
    {
        deltas = effectName switch
        {
            "modifyGlory" => new[] { ("glory", value) },
            "modifyMilitarySkill" => new[] { ("military", value) },
            "modifyPoliticalSkill" => new[] { ("political", value) },
            "modifyBothSkills" => new[] { ("military", value), ("political", value) },
            "modifyProvinceStrength" => new[] { ("provinceStrength", value) },
            _ => Array.Empty<(string, int)>()
        };
        return deltas.Count > 0;
    }

    public static bool TryGetRestrictionAction(string? effectName, JsonElement? value, out string action)
    {
        switch (effectName)
        {
            case "cardCannot":
                action = ParseCannotValue(value!.Value);
                return true;
            case "cannotParticipateAsAttacker":
                action = "declareAsAttacker";
                return true;
            case "cannotParticipateAsDefender":
                action = "declareAsDefender";
                return true;
            default:
                action = "";
                return false;
        }
    }

    /// <summary>cardCannot's value is either a bare string (hiruma-yojimbo/aggressive-moto) or an object with a "cannot" key (hiruma-ambusher/tranquility). "restricts" (who the restriction applies against, e.g. "opponentsCardEffects") isn't supported yet - throws rather than silently applying a broader-than-intended restriction.</summary>
    private static string ParseCannotValue(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.String)
            return value.GetString()!;

        if (value.TryGetProperty("restricts", out _))
            throw new NotSupportedException("cardCannot's 'restricts' qualifier (scoping who the restriction applies against) isn't supported yet.");

        return value.GetProperty("cannot").GetString()!;
    }
}
