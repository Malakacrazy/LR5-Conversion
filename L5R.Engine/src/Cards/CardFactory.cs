using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Cards;

/// <summary>
/// Builds a real, playable State.Card from a card's JSON document - the bridge CardLoader
/// deliberately doesn't provide (its own doc comment: "not full runtime instantiation, ...
/// needs the state model to grow ... before it means anything"). It has now grown enough.
///
/// Printed stats/traits/keywords are read generically here for the first time - every
/// PerCard test until now hand-transcribed them onto a Card built in the test itself.
/// PersistentEffects/WhileAttachedEffects/TriggeredAbilities reuse AbilityDefinitionParser
/// directly, already exactly the right shape. The one genuinely new piece is bridging
/// ActionDefinition into Card.Actions (so LegalActions.GetLegalActions can enumerate a real
/// card's actions) via the JsonActionCost/JsonActionTarget adapters - see their own doc
/// comments for why a direct CardAction can't just wrap an ActionDefinition's Costs/Target
/// fields as-is (different representations of the same information).
/// </summary>
public static class CardFactory
{
    public static Card BuildCard(JsonElement cardJson, Player controller)
    {
        var costRegistry = new CostRegistry();
        var executor = new AbilityExecutor(costRegistry, new GameActionRegistry());

        var card = new Card
        {
            Id = cardJson.GetProperty("id").GetString()!,
            Type = CardTypes.Parse(cardJson.GetProperty("type").GetString()!),
            Controller = controller,
            Faction = ReadNullableString(cardJson, "faction"),
            Unique = cardJson.TryGetProperty("unique", out var unique) && unique.GetBoolean(),
            Traits = ReadStringArray(cardJson, "traits"),
            PrintedKeywords = ReadStringArray(cardJson, "keywords"),
            PrintedCost = ReadNullableInt(cardJson, "printedCost"),
            PrintedMilitarySkill = ReadNullableInt(cardJson, "military"),
            PrintedPoliticalSkill = ReadNullableInt(cardJson, "political"),
            PrintedGlory = ReadNullableInt(cardJson, "glory"),
            PrintedProvinceStrength = ReadNullableInt(cardJson, "strength"),
            PrintedStrengthBonus = ReadNullableInt(cardJson, "strengthBonus"),
            PrintedMilitaryBonus = ReadNullableInt(cardJson, "militaryBonus"),
            PrintedPoliticalBonus = ReadNullableInt(cardJson, "politicalBonus"),
            PrintedFateIncome = ReadNullableInt(cardJson, "fate"),
            PrintedHonor = ReadNullableInt(cardJson, "honor"),
            PersistentEffects = AbilityDefinitionParser.ParsePersistentEffects(cardJson),
            WhileAttachedEffects = AbilityDefinitionParser.ParseWhileAttached(cardJson),
            TriggeredAbilities = AbilityDefinitionParser.ParseTriggeredAbilities(cardJson)
        };

        foreach (var action in AbilityDefinitionParser.ParseActions(cardJson))
            card.Actions.Add(BuildCardAction(action, card, executor, costRegistry));

        if (cardJson.TryGetProperty("scriptOverride", out var scriptOverride))
            card.PlayScript = BuildScript(card.Id, scriptOverride);

        return card;
    }

    private static CardAction BuildCardAction(ActionDefinition definition, Card card, AbilityExecutor executor, CostRegistry costRegistry) =>
        new()
        {
            Title = definition.Title,
            Card = card,
            ValidLocations = new[] { definition.Location },
            Phase = definition.Phase is { } phase ? Phases.Parse(phase) : null,
            Condition = context => executor.IsConditionMet(definition, context),
            Costs = definition.Costs.Select(c => (ICost)new JsonActionCost(c, costRegistry)).ToArray(),
            Targets = definition.Target is { } target ? new ITargetRequirement[] { new JsonActionTarget(target) } : Array.Empty<ITargetRequirement>(),
            Definition = definition
        };

    /// <summary>Same reflection CardLoader.LoadScriptOverride uses to validate the handler type, but instantiates it - every ICardScript ported so far is a parameterless sealed class.</summary>
    private static ICardScript BuildScript(string cardId, JsonElement scriptOverride)
    {
        var handlerName = scriptOverride.GetProperty("handler").GetString()!;
        var handlerType = Type.GetType(handlerName)
            ?? throw new CardLoadException($"Card '{cardId}': scriptOverride handler '{handlerName}' could not be resolved.");

        return (ICardScript)Activator.CreateInstance(handlerType)!;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement cardJson, string property) =>
        cardJson.TryGetProperty(property, out var array)
            ? array.EnumerateArray().Select(e => e.GetString()!).ToArray()
            : Array.Empty<string>();

    private static string? ReadNullableString(JsonElement cardJson, string property) =>
        cardJson.TryGetProperty(property, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.GetString()
            : null;

    private static int? ReadNullableInt(JsonElement cardJson, string property) =>
        cardJson.TryGetProperty(property, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.GetInt32()
            : null;
}
