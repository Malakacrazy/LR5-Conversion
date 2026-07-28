using L5R.Engine.Cards;

namespace L5R.Engine.Tests.Cards;

public class CardLoaderTests
{
    private static CardLoader NewLoader() =>
        new(RingtekiCatalog.Effects, RingtekiCatalog.GameActions, RingtekiCatalog.Costs);

    [Test]
    public void Load_ReadsTopLevelFields()
    {
        var loader = NewLoader();

        var card = loader.Load("""{ "id": "example-card", "name": "Example Card", "type": "character" }""");

        Assert.Multiple(() =>
        {
            Assert.That(card.Id, Is.EqualTo("example-card"));
            Assert.That(card.Name, Is.EqualTo("Example Card"));
            Assert.That(card.Type, Is.EqualTo("character"));
        });
    }

    [Test]
    public void Load_CollectsEffectGameActionAndCostReferences_IncludingNestedInsideParams()
    {
        var loader = NewLoader();
        var json = """
            {
              "id": "example-card",
              "name": "Example Card",
              "type": "character",
              "abilities": {
                "persistentEffects": [
                  { "match": "self", "effect": { "name": "addKeyword" } }
                ],
                "actions": [
                  {
                    "title": "Do a thing",
                    "cost": [{ "name": "bowSelf" }],
                    "gameAction": {
                      "name": "cardLastingEffect",
                      "params": { "duration": "untilEndOfConflict", "effect": { "name": "modifyMilitarySkill", "value": 1 } }
                    }
                  }
                ]
              }
            }
            """;

        var card = loader.Load(json);

        Assert.That(card.References, Is.EquivalentTo(new[]
        {
            new CollectedReference("effect", "addKeyword"),
            new CollectedReference("cost", "bowSelf"),
            new CollectedReference("gameAction", "cardLastingEffect"),
            new CollectedReference("effect", "modifyMilitarySkill")
        }));
    }

    [Test]
    public void Load_Throws_WhenAnEffectNameIsNotRegistered()
    {
        var loader = NewLoader();
        var json = """
            {
              "id": "example-card",
              "name": "Example Card",
              "type": "character",
              "abilities": {
                "persistentEffects": [
                  { "match": "self", "effect": { "name": "thisIsNotARealEffect" } }
                ]
              }
            }
            """;

        Assert.That(() => loader.Load(json), Throws.TypeOf<CardLoadException>()
            .With.Message.Contains("thisIsNotARealEffect"));
    }

    [Test]
    public void Load_DoesNotMistakeTheCardsOwnNameForADslReference()
    {
        // "name" is the reference key inside effect/gameAction/cost entries, but the
        // card's own top-level "name" field must never be swept up by the same scan.
        var loader = NewLoader();
        var json = """{ "id": "example-card", "name": "modifyMilitarySkill", "type": "character" }""";

        var card = loader.Load(json);

        Assert.That(card.References, Is.Empty);
    }

    [Test]
    public void Load_ResolvesAScriptOverrideHandler()
    {
        var loader = NewLoader();
        var handlerTypeName = typeof(FakeCardScript).AssemblyQualifiedName;
        var json = $$"""
            {
              "id": "example-card",
              "name": "Example Card",
              "type": "character",
              "scriptOverride": { "reason": "test fixture", "handler": "{{handlerTypeName}}" }
            }
            """;

        var card = loader.Load(json);

        Assert.That(card.ScriptOverride, Is.Not.Null);
        Assert.That(card.ScriptOverride!.HandlerType, Is.EqualTo(typeof(FakeCardScript)));
        Assert.That(card.ScriptOverride!.Reason, Is.EqualTo("test fixture"));
    }

    [Test]
    public void Load_Throws_WhenScriptOverrideHandlerDoesNotImplementICardScript()
    {
        var loader = NewLoader();
        var handlerTypeName = typeof(string).AssemblyQualifiedName;
        var json = $$"""
            {
              "id": "example-card",
              "name": "Example Card",
              "type": "character",
              "scriptOverride": { "reason": "test fixture", "handler": "{{handlerTypeName}}" }
            }
            """;

        Assert.That(() => loader.Load(json), Throws.TypeOf<CardLoadException>());
    }

    private sealed class FakeCardScript : ICardScript
    {
    }
}
