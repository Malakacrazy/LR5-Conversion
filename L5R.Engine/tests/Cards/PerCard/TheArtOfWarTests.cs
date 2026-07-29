using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class TheArtOfWarTests
{
    private static TriggeredAbilityDefinition LoadFirstTriggeredAbility()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "the-art-of-war.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseTriggeredAbilities(document.RootElement).Single();
    }

    [Test]
    public void WhenThisProvinceBreaks_DrawsThreeCards()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var province = new Card { Id = "the-art-of-war", Type = CardType.Province, Controller = p1 };
        p1.Deck.AddRange(new[]
        {
            new Card { Id = "deck-1", Type = CardType.Character, Controller = p1 },
            new Card { Id = "deck-2", Type = CardType.Character, Controller = p1 },
            new Card { Id = "deck-3", Type = CardType.Character, Controller = p1 },
        });

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = province };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.ExecuteTriggered(ability, context, eventCard: province);

        Assert.That(p1.Hand, Has.Count.EqualTo(3));
        Assert.That(p1.Deck, Is.Empty);
    }
}
