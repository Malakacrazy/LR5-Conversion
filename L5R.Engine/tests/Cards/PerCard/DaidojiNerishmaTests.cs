using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class DaidojiNerishmaTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "daidoji-nerishma.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void FlipsTheChosenFacedownProvinceFaceup()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var nerishma = new Card { Id = "daidoji-nerishma", Type = CardType.Character, Controller = p1 };
        var province = new Card { Id = "some-province", Type = CardType.Holding, Controller = p1, Location = "province", Facedown = true };
        p1.PlayArea.Add(nerishma);
        p1.Provinces.Add(province);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = nerishma };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: province);

        Assert.That(province.Facedown, Is.False);
    }

    [Test]
    public void AFaceupProvince_IsNotALegalTarget()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var nerishma = new Card { Id = "daidoji-nerishma", Type = CardType.Character, Controller = p1 };
        var facedown = new Card { Id = "facedown-province", Type = CardType.Holding, Controller = p1, Location = "province", Facedown = true };
        var faceup = new Card { Id = "faceup-province", Type = CardType.Holding, Controller = p1, Location = "province", Facedown = false };
        p1.PlayArea.Add(nerishma);
        p1.Provinces.Add(facedown);
        p1.Provinces.Add(faceup);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = nerishma };
        var legalTargets = TargetResolver.ResolveLegalTargets(action.Target!, context);

        Assert.That(legalTargets, Does.Contain(facedown));
        Assert.That(legalTargets, Does.Not.Contain(faceup));
    }

    [Test]
    public void AnOpponentsProvince_IsNotALegalTarget()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var nerishma = new Card { Id = "daidoji-nerishma", Type = CardType.Character, Controller = p1 };
        var opponentProvince = new Card { Id = "opponent-province", Type = CardType.Holding, Controller = p2, Location = "province", Facedown = true };
        p1.PlayArea.Add(nerishma);
        p2.Provinces.Add(opponentProvince);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = nerishma };
        var legalTargets = TargetResolver.ResolveLegalTargets(action.Target!, context);

        Assert.That(legalTargets, Does.Not.Contain(opponentProvince), "location: self");
    }
}
