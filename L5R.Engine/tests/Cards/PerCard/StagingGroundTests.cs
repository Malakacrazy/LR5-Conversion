using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class StagingGroundTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "staging-ground.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    private static (GameState Game, Card StagingGround, Card ProvinceOne, Card ProvinceTwo) NewGame()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var stagingGround = new Card { Id = "staging-ground", Type = CardType.Holding, Controller = p1 };
        var provinceOne = new Card { Id = "province-one", Type = CardType.Holding, Controller = p1, Location = "province", Facedown = true };
        var provinceTwo = new Card { Id = "province-two", Type = CardType.Holding, Controller = p1, Location = "province", Facedown = true };
        p1.PlayArea.Add(stagingGround);
        p1.Provinces.Add(provinceOne);
        p1.Provinces.Add(provinceTwo);
        return (game, stagingGround, provinceOne, provinceTwo);
    }

    [Test]
    public void FlipsUpToTwoChosenFacedownProvinces()
    {
        var (game, stagingGround, provinceOne, provinceTwo) = NewGame();

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = stagingGround };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTargets: new[] { provinceOne, provinceTwo });

        Assert.That(provinceOne.Facedown, Is.False);
        Assert.That(provinceTwo.Facedown, Is.False);
    }

    [Test]
    public void FlippingJustOneIsAlsoLegal()
    {
        var (game, stagingGround, provinceOne, provinceTwo) = NewGame();

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = stagingGround };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTargets: new[] { provinceOne });

        Assert.That(provinceOne.Facedown, Is.False);
        Assert.That(provinceTwo.Facedown, Is.True, "not chosen");
    }

    [Test]
    public void CannotChooseMoreThanTwo()
    {
        var (game, stagingGround, provinceOne, provinceTwo) = NewGame();
        var provinceThree = new Card { Id = "province-three", Type = CardType.Holding, Controller = game.Player1, Location = "province", Facedown = true };
        game.Player1.Provinces.Add(provinceThree);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = stagingGround };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(
            () => executor.Execute(action, context, chosenTargets: new[] { provinceOne, provinceTwo, provinceThree }));
    }
}
