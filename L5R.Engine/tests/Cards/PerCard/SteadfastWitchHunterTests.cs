using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SteadfastWitchHunterTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "steadfast-witch-hunter.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void SacrificingAnotherOwnCharacterReadiesTheChosenTarget()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var witchHunter = new Card { Id = "steadfast-witch-hunter", Type = CardType.Character, Controller = p1 };
        var fodder = new Card { Id = "fodder", Type = CardType.Character, Controller = p1 };
        var bowedTarget = new Card { Id = "bowed-target", Type = CardType.Character, Controller = p1, Bowed = true };
        p1.PlayArea.Add(witchHunter);
        p1.PlayArea.Add(fodder);
        p1.PlayArea.Add(bowedTarget);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = witchHunter };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: bowedTarget, chosenCostTarget: fodder);

        Assert.That(p1.Discard, Does.Contain(fodder));
        Assert.That(bowedTarget.Bowed, Is.False);
    }

    [Test]
    public void CanSacrificeItselfAsTheCost()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var witchHunter = new Card { Id = "steadfast-witch-hunter", Type = CardType.Character, Controller = p1 };
        var bowedTarget = new Card { Id = "bowed-target", Type = CardType.Character, Controller = p1, Bowed = true };
        p1.PlayArea.Add(witchHunter);
        p1.PlayArea.Add(bowedTarget);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = witchHunter };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        // ringteki's sacrifice cost has no exclusion of context.source from its selector -
        // a character can pay its own "sacrifice a character" cost with itself.
        executor.Execute(action, context, chosenTarget: bowedTarget, chosenCostTarget: witchHunter);

        Assert.That(p1.Discard, Does.Contain(witchHunter));
        Assert.That(bowedTarget.Bowed, Is.False);
    }
}
