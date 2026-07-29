using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ContingencyPlanTests
{
    private static TriggeredAbilityDefinition LoadFirstTriggeredAbility()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "contingency-plan.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseTriggeredAbilities(document.RootElement).Single();
    }

    [Test]
    public void ChoosingToIncrease_RaisesTheBidByOne()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var source = new Card { Id = "contingency-plan", Type = CardType.Event, Controller = p1 };

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = source, ChosenBidDirection = "increase" };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.ExecuteTriggered(ability, context, eventCard: source);

        Assert.That(p1.HonorBidModifier, Is.EqualTo(1));
    }

    [Test]
    public void ChoosingToDecrease_LowersTheBidByOne()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var source = new Card { Id = "contingency-plan", Type = CardType.Event, Controller = p1 };

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = source, ChosenBidDirection = "decrease" };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.ExecuteTriggered(ability, context, eventCard: source);

        Assert.That(p1.HonorBidModifier, Is.EqualTo(-1));
    }

    [Test]
    public void WithNoChosenDirection_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var source = new Card { Id = "contingency-plan", Type = CardType.Event, Controller = p1 };

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.ExecuteTriggered(ability, context, eventCard: source));
    }
}
