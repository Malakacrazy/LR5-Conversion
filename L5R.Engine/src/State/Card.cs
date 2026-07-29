namespace L5R.Engine.State;

/// <summary>
/// Minimal card state - location/controller/bowed/traits, enough for CardAction's
/// requirement checks (location, phase, player, condition), plus Fate/Faction/Unique/
/// PrintedCost/IsHonored/IsDishonored for the predicate evaluator's first ops
/// (hasFaction, compareStat, hasStatus). Grows further as later card groups need more
/// (skill values, attachments, persistent effects, etc).
/// </summary>
public sealed class Card
{
    public required string Id { get; init; }
    public required CardType Type { get; init; }
    public required Player Controller { get; set; }
    public string Location { get; set; } = "play area";
    public bool Bowed { get; set; }
    public int Fate { get; set; }
    public string? Faction { get; init; }
    public bool Unique { get; init; }
    public int? PrintedCost { get; init; }
    public int? PrintedGlory { get; init; }
    public int? PrintedMilitarySkill { get; init; }
    public int? PrintedPoliticalSkill { get; init; }
    public int? PrintedProvinceStrength { get; init; }
    public bool IsHonored { get; set; }
    public bool IsDishonored { get; set; }
    public IReadOnlyList<string> Traits { get; init; } = Array.Empty<string>();
    public List<Abilities.CardAction> Actions { get; } = new();

    /// <summary>
    /// Parsed abilities.persistentEffects[] (see PersistentEffectDefinition's own doc
    /// comment) - empty by default, since only tests that specifically exercise a card's
    /// persistent effects populate it (via AbilityDefinitionParser.ParsePersistentEffects).
    /// GameState scans every in-play card's list on demand; it isn't tracked as
    /// materialized state anywhere.
    /// </summary>
    public IReadOnlyList<Dsl.PersistentEffectDefinition> PersistentEffects { get; init; } = Array.Empty<Dsl.PersistentEffectDefinition>();

    /// <summary>
    /// The character this attachment is attached to, null otherwise (and always null for
    /// non-attachment cards). No "attach" gameAction exists yet - set directly by the
    /// caller, like every other zone/relationship field. Drives WhileAttachedDefinition
    /// evaluation (see Card.WhileAttachedEffects and GameState's whileAttached scan) and
    /// the "source.parent" contextPath.
    /// </summary>
    public Card? AttachedTo { get; set; }

    /// <summary>
    /// Parsed abilities.whileAttached[] (see WhileAttachedDefinition's own doc comment) -
    /// empty by default, populated only by tests exercising a card's whileAttached effects,
    /// same convention as PersistentEffects.
    /// </summary>
    public IReadOnlyList<Dsl.WhileAttachedDefinition> WhileAttachedEffects { get; init; } = Array.Empty<Dsl.WhileAttachedDefinition>();
}
