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
    private static readonly HashSet<string> StatEffectNames = new()
    {
        "modifyGlory", "modifyMilitarySkill", "modifyPoliticalSkill", "modifyBothSkills", "modifyProvinceStrength"
    };

    /// <summary>
    /// A name-only check, deliberately separate from TryGetStatDeltas: a passive scan (e.g.
    /// GameState.EffectiveStat) needs to know whether an effect is stat-shaped *before*
    /// trying to resolve its "value" as an int - cardCannot's value is an object, addTrait/
    /// addKeyword's is a string, and blindly calling ValueRefResolver.ResolveInt on those
    /// throws, even though the effect is simply irrelevant to a stat query, not malformed.
    /// </summary>
    public static bool IsStatEffect(string? effectName) => effectName is not null && StatEffectNames.Contains(effectName);

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

    /// <summary>
    /// qualifier is non-null only for cannotParticipateAsAttacker/Defender's optional
    /// conflict-type/element scoping (pacifism's "military") - null means unconditional.
    /// cardCannot/doesNotBow never carry one (no ported card qualifies them).
    /// </summary>
    public static bool TryGetRestrictionAction(string? effectName, JsonElement? value, out string action, out string? qualifier)
    {
        qualifier = null;
        switch (effectName)
        {
            case "cardCannot":
                action = ParseCannotValue(value!.Value);
                return true;
            case "cannotParticipateAsAttacker":
                action = "declareAsAttacker";
                qualifier = value?.GetString();
                return true;
            case "cannotParticipateAsDefender":
                action = "declareAsDefender";
                qualifier = value?.GetString();
                return true;
            case "doesNotBow":
                action = "bow";
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
