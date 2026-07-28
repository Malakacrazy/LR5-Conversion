using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class AdeptOfShadowsTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "adept-of-shadows.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void PaysOneHonorToReturnItselfToItsControllersHand()
    {
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var adeptOfShadows = new Card { Id = "adept-of-shadows", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(adeptOfShadows);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = adeptOfShadows };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(p1.Honor, Is.EqualTo(4), "payHonor(1) reduces the acting player's honor");
        Assert.That(p1.PlayArea, Does.Not.Contain(adeptOfShadows));
        Assert.That(p1.Hand, Does.Contain(adeptOfShadows), "returnToHand with no explicit target defaults to source");
    }
}
