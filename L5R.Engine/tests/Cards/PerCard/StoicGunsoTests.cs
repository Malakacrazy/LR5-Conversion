using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class StoicGunsoTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "stoic-gunso.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void SacrificingACharacter_GivesItselfPlusThreeMilitarySkill()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var gunso = new Card { Id = "stoic-gunso", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2 };
        var fodder = new Card { Id = "fodder", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(gunso);
        p1.PlayArea.Add(fodder);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = gunso };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenCostTarget: fodder);

        Assert.That(p1.Discard, Does.Contain(fodder), "sacrifice cost was paid");
        Assert.That(game.EffectiveMilitarySkill(gunso), Is.EqualTo(5));
        Assert.That(game.EffectivePoliticalSkill(gunso), Is.EqualTo(0), "only military skill is modified");
    }
}
