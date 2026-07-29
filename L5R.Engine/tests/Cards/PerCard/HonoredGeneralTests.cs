using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class HonoredGeneralTests
{
    private static TriggeredAbilityDefinition LoadFirstTriggeredAbility()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "honored-general.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseTriggeredAbilities(document.RootElement).Single();
    }

    [Test]
    public void WhenThisCharacterEntersPlay_ItIsHonored()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var general = new Card { Id = "honored-general", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(general);

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = general };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        // No event bus yet - asserting the trigger fired means passing the card that
        // entered play (itself) as the event's subject.
        executor.ExecuteTriggered(ability, context, eventCard: general);

        Assert.That(general.IsHonored, Is.True);
    }

    [Test]
    public void DoesNotFire_WhenTheEnteringCharacterIsNotThisOne()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var general = new Card { Id = "honored-general", Type = CardType.Character, Controller = p1 };
        var someoneElse = new Card { Id = "someone-else", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(general);
        p1.PlayArea.Add(someoneElse);

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = general };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.ExecuteTriggered(ability, context, eventCard: someoneElse));
        Assert.That(general.IsHonored, Is.False);
    }
}
