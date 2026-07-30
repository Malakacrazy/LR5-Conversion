using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Cards;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards;

/// <summary>
/// Proves CardFactory.BuildCard - the JSON-to-live-Card bridge CardLoader deliberately
/// doesn't provide - actually works against every real ported card, not just hand-built
/// fixtures. Unlike CoreSetCardsTests/CoreSetCardsSchemaValidationTests' hand-maintained
/// [TestCase] lists (kept that way so each entry can carry its own scriptOverride-handler-type
/// assertion), this is a blanket smoke test with no per-card assertions to keep in sync, so it
/// enumerates the real Cards/01-Core directory directly instead of adding a third parallel list.
/// </summary>
public class CardFactoryTests
{
    private static readonly string CardsDir = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core");

    private static IEnumerable<string> AllCoreSetCardIds() =>
        Directory.GetFiles(CardsDir, "*.json").Select(Path.GetFileNameWithoutExtension)!;

    private static JsonElement LoadJson(string cardId)
    {
        var path = Path.Combine(CardsDir, $"{cardId}.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.Clone();
    }

    [TestCaseSource(nameof(AllCoreSetCardIds))]
    public void EveryRealCoreSetCard_BuildsWithoutThrowing(string cardId)
    {
        var controller = new Player { Name = "Player1" };

        Assert.DoesNotThrow(() => CardFactory.BuildCard(LoadJson(cardId), controller));
    }

    [Test]
    public void VanillaCharacter_HasItsPrintedStatsTraitsAndKeywords()
    {
        var controller = new Player { Name = "Player1" };

        var liar = CardFactory.BuildCard(LoadJson("bayushi-liar"), controller);

        Assert.That(liar.Id, Is.EqualTo("bayushi-liar"));
        Assert.That(liar.Type, Is.EqualTo(CardType.Character));
        Assert.That(liar.Controller, Is.EqualTo(controller));
        Assert.That(liar.Faction, Is.EqualTo("scorpion"));
        Assert.That(liar.Traits, Is.EquivalentTo(new[] { "courtier" }));
        Assert.That(liar.PrintedKeywords, Is.EquivalentTo(new[] { "sincerity" }));
        Assert.That(liar.PrintedMilitarySkill, Is.Null, "no printed military skill - a dash");
        Assert.That(liar.PrintedPoliticalSkill, Is.EqualTo(3));
        Assert.That(liar.PrintedGlory, Is.EqualTo(0));
    }

    [Test]
    public void PersistentEffectCard_ParsesItsPersistentEffects()
    {
        var controller = new Player { Name = "Player1" };

        var motoYouth = CardFactory.BuildCard(LoadJson("moto-youth"), controller);

        Assert.That(motoYouth.PersistentEffects, Has.Count.EqualTo(1));
    }

    [Test]
    public void ScriptOverrideCard_ResolvesItsPlayScript()
    {
        var controller = new Player { Name = "Player1" };

        var wanderer = CardFactory.BuildCard(LoadJson("tattooed-wanderer"), controller);

        Assert.That(wanderer.PlayScript, Is.InstanceOf<TattooedWandererPlayAsAttachment>());
    }

    [Test]
    public void ActionBearingCard_BuildsAResolvableCardAction()
    {
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };

        var adept = CardFactory.BuildCard(LoadJson("adept-of-shadows"), p1);
        p1.PlayArea.Add(adept);

        Assert.That(adept.Actions, Has.Count.EqualTo(1));
        var action = adept.Actions[0];
        Assert.That(action.Definition, Is.Not.Null);

        var context = new AbilityContext { Game = game, Player = p1, Source = adept };
        Assert.That(action.MeetsRequirements(context), Is.True, "affordable and no target required");

        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());
        executor.Execute(action.Definition!, context);

        Assert.That(p1.Honor, Is.EqualTo(4));
        Assert.That(p1.Hand, Does.Contain(adept));
    }

    [Test]
    public void ActionBearingCard_IsIllegalWhenItsCostCannotBePaid()
    {
        var p1 = new Player { Name = "Player1", Honor = 0 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };

        var adept = CardFactory.BuildCard(LoadJson("adept-of-shadows"), p1);
        p1.PlayArea.Add(adept);

        var context = new AbilityContext { Game = game, Player = p1, Source = adept };

        Assert.That(adept.Actions[0].MeetsRequirements(context), Is.False, "can't pay 1 honor with 0");
    }
}
