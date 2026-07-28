using System.Text.Json;
using L5R.Engine.Tests.Schema;

namespace L5R.Engine.Tests.Cards;

/// <summary>
/// Every authored Core Set card must also validate against card-schema.json itself, not
/// just load through CardLoader - the two checks catch different things (CardLoader
/// checks names are registered; the schema checks shape/structure).
/// </summary>
public class CoreSetCardsSchemaValidationTests
{
    private static readonly string CardsDir = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core");

    [TestCase("hiruma-yojimbo")]
    [TestCase("border-rider")]
    [TestCase("cloud-the-mind")]
    [TestCase("assassination")]
    [TestCase("court-mask")]
    [TestCase("ancestral-lands")]
    [TestCase("city-of-the-open-hand")]
    [TestCase("artisan-academy")]
    [TestCase("keeper-of-air")]
    [TestCase("magnificent-kimono")]
    [TestCase("for-shame")]
    [TestCase("steadfast-samurai")]
    [TestCase("shiba-yojimbo")]
    [TestCase("favored-niece")]
    [TestCase("agasha-swordsmith")]
    [TestCase("shiba-tsukune")]
    [TestCase("niten-master")]
    [TestCase("guest-of-honor")]
    [TestCase("hida-guardian")]
    [TestCase("grasp-of-earth")]
    [TestCase("cautious-scout")]
    public void RealCoreSetCard_ValidatesAgainstTheSchema(string cardId)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(CardsDir, $"{cardId}.json")));

        var result = SharedCardSchema.Instance.Evaluate(document.RootElement);

        Assert.That(result.IsValid, Is.True, $"{cardId}.json must validate against card-schema.json");
    }
}
