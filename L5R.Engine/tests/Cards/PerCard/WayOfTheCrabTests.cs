using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class WayOfTheCrabTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "way-of-the-crab.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void SacrificingOwnCrabForcesOpponentToSacrificeTheChosenCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "way-of-the-crab", Type = CardType.Event, Controller = p1 };
        var ownCrab = new Card { Id = "own-crab", Type = CardType.Character, Controller = p1, Faction = "crab" };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(ownCrab);
        p2.PlayArea.Add(opponentCharacter);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: opponentCharacter, chosenCostTarget: ownCrab);

        Assert.That(p1.Discard, Does.Contain(ownCrab), "sacrifice cost requires an own crab character");
        Assert.That(p2.Discard, Does.Contain(opponentCharacter), "the target's own gameAction is also named sacrifice - same DiscardFromPlayAction class as discardFromPlay");
    }

    [Test]
    public void CannotPayCost_WithoutAnOwnCrabCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "way-of-the-crab", Type = CardType.Event, Controller = p1 };
        var ownScorpion = new Card { Id = "own-scorpion", Type = CardType.Character, Controller = p1, Faction = "scorpion" };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(ownScorpion);
        p2.PlayArea.Add(opponentCharacter);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(
            () => executor.Execute(action, context, chosenTarget: opponentCharacter));
    }
}
