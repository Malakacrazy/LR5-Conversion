using System.Text.Json;
using Json.Schema;

namespace L5R.Engine.Tests.Schema;

/// <summary>
/// Proves card-schema.json is actually usable, not just hand-inspected: the worked
/// example must validate, and a document missing something the roadmap requires
/// (id/name/type, or an unknown top-level field) must not.
/// </summary>
public class CardSchemaTests
{
    private static readonly string SchemaDir = Path.Combine(AppContext.BaseDirectory, "Schema");

    // JsonSchema.Net registers schemas globally by $id; parsing the same file more than
    // once throws "Overwriting registered schemas is not permitted." Load it once and
    // share it across tests.
    private static readonly JsonSchema Schema =
        JsonSchema.FromFile(Path.Combine(SchemaDir, "card-schema.json"));

    private static JsonSchema LoadSchema() => Schema;

    [Test]
    public void WorkedExample_ValidatesAgainstTheSchema()
    {
        var schema = LoadSchema();
        using var example = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(SchemaDir, "examples", "lantern-keeper.example.json")));

        var result = schema.Evaluate(example.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.List });

        Assert.That(result.IsValid, Is.True,
            "worked example must validate: " + string.Join("; ", CollectErrors(result)));
    }

    [Test]
    public void DocumentMissingRequiredFields_FailsValidation()
    {
        var schema = LoadSchema();
        using var invalid = JsonDocument.Parse("""{ "name": "Nameless" }""");

        var result = schema.Evaluate(invalid.RootElement);

        Assert.That(result.IsValid, Is.False, "a card without id/type must not validate");
    }

    [Test]
    public void DocumentWithAnUnknownTopLevelField_FailsValidation()
    {
        // additionalProperties: false is deliberate - a typoed field name should be a
        // loud authoring error, not a silently-ignored no-op when porting ~223 cards.
        var schema = LoadSchema();
        using var invalid = JsonDocument.Parse("""
            {
              "id": "example-card",
              "name": "Example Card",
              "type": "character",
              "printedCostt": 3
            }
            """);

        var result = schema.Evaluate(invalid.RootElement);

        Assert.That(result.IsValid, Is.False, "a typoed/unknown field must not silently pass");
    }

    [Test]
    public void EffectAndGameActionNames_AreNotRestrictedByTheSchema()
    {
        // Deliberate design choice documented in card-schema.md: name validity is an
        // engine registry concern at card-load time, not a schema enum, so the two can't
        // drift apart. This test pins that choice down so nobody "fixes" it into an enum.
        var schema = LoadSchema();
        using var document = JsonDocument.Parse("""
            {
              "id": "example-card",
              "name": "Example Card",
              "type": "character",
              "abilities": {
                "persistentEffects": [
                  { "match": "self", "effect": { "name": "thisEffectNameDoesNotExistInAnyCatalog" } }
                ]
              }
            }
            """);

        var result = schema.Evaluate(document.RootElement);

        Assert.That(result.IsValid, Is.True,
            "schema only checks shape; unknown effect names are caught by the engine's EffectRegistry at load time, not here");
    }

    private static IEnumerable<string> CollectErrors(EvaluationResults result)
    {
        // Only descend into branches that actually failed - a oneOf's non-matching
        // alternatives are expected to "fail" and aren't the real problem.
        if (result.IsValid)
            yield break;

        if (result.Errors is not null)
            foreach (var error in result.Errors)
                yield return $"{result.InstanceLocation}: {error.Key}={error.Value}";

        if (result.Details is not null)
            foreach (var detail in result.Details)
                foreach (var error in CollectErrors(detail))
                    yield return error;
    }
}
