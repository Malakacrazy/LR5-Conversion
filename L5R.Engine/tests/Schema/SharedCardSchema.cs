using Json.Schema;

namespace L5R.Engine.Tests.Schema;

/// <summary>
/// JsonSchema.Net registers schemas globally by $id; loading card-schema.json more than
/// once anywhere in the test run throws "Overwriting registered schemas is not
/// permitted." Every test class that needs the schema shares this single instance.
/// </summary>
public static class SharedCardSchema
{
    public static readonly string SchemaPath = Path.Combine(AppContext.BaseDirectory, "Schema", "card-schema.json");

    public static readonly JsonSchema Instance = JsonSchema.FromFile(SchemaPath);
}
