using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class CourtMaskTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "court-mask.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void ReturnsItselfToHandAndDishonorsTheWearer()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var wearer = new Card { Id = "wearer", Type = CardType.Character, Controller = p1 };
        var mask = new Card { Id = "court-mask", Type = CardType.Attachment, Controller = p1, AttachedTo = wearer };
        p1.PlayArea.Add(wearer);
        p1.PlayArea.Add(mask);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = mask };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(p1.Hand, Does.Contain(mask));
        Assert.That(p1.PlayArea, Does.Not.Contain(mask));
        Assert.That(wearer.IsDishonored, Is.True);
    }

    [Test]
    public void WhenNotAttached_ThrowsRatherThanSilentlyTargetingNothing()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var mask = new Card { Id = "court-mask", Type = CardType.Attachment, Controller = p1 };
        p1.PlayArea.Add(mask);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = mask };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context));
    }
}
