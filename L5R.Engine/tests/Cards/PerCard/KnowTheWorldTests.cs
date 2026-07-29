using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class KnowTheWorldTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "know-the-world.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void SwitchesAClaimedRingTheControllerHoldsForAnUnclaimedOne()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var fireRing = game.Rings.Single(r => r.Element == "fire");
        var waterRing = game.Rings.Single(r => r.Element == "water");
        fireRing.Claimed = true;
        fireRing.ClaimedBy = p1;
        waterRing.Fate = 2;

        var source = new Card { Id = "know-the-world", Type = CardType.Event, Controller = p1 };
        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenRingTargets: new Dictionary<string, Ring>
        {
            ["returnedRing"] = fireRing,
            ["takenRing"] = waterRing
        });

        Assert.That(fireRing.Claimed, Is.False);
        Assert.That(fireRing.ClaimedBy, Is.Null);
        Assert.That(waterRing.Claimed, Is.True);
        Assert.That(waterRing.ClaimedBy, Is.EqualTo(p1));
        Assert.That(waterRing.Fate, Is.EqualTo(0));
        Assert.That(p1.Fate, Is.EqualTo(2), "takeFate: true moves the taken ring's fate to the controller");
    }

    [Test]
    public void CannotReturnARingItDoesNotControl()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var fireRing = game.Rings.Single(r => r.Element == "fire");
        var waterRing = game.Rings.Single(r => r.Element == "water");
        fireRing.Claimed = true;
        fireRing.ClaimedBy = p2;

        var source = new Card { Id = "know-the-world", Type = CardType.Event, Controller = p1 };
        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context, chosenRingTargets: new Dictionary<string, Ring>
        {
            ["returnedRing"] = fireRing,
            ["takenRing"] = waterRing
        }));
    }

    [Test]
    public void CannotTakeARingThatIsAlreadyClaimed()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var fireRing = game.Rings.Single(r => r.Element == "fire");
        var waterRing = game.Rings.Single(r => r.Element == "water");
        fireRing.Claimed = true;
        fireRing.ClaimedBy = p1;
        waterRing.Claimed = true;
        waterRing.ClaimedBy = p2;

        var source = new Card { Id = "know-the-world", Type = CardType.Event, Controller = p1 };
        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context, chosenRingTargets: new Dictionary<string, Ring>
        {
            ["returnedRing"] = fireRing,
            ["takenRing"] = waterRing
        }));
    }
}
