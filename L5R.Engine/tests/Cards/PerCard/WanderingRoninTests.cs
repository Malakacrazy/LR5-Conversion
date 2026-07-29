using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class WanderingRoninTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "wandering-ronin.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringConflict_RemovingAFateGivesItselfPlusTwoPlusTwo()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var ronin = new Card { Id = "wandering-ronin", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2, PrintedPoliticalSkill = 2, Fate = 1 };
        p1.PlayArea.Add(ronin);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = ronin };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(ronin.Fate, Is.EqualTo(0), "removeFateFromSelf cost was paid");
        Assert.That(game.EffectiveMilitarySkill(ronin), Is.EqualTo(4));
        Assert.That(game.EffectivePoliticalSkill(ronin), Is.EqualTo(4));
    }

    [Test]
    public void WithNoFate_CannotPayTheCost()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var ronin = new Card { Id = "wandering-ronin", Type = CardType.Character, Controller = p1, Fate = 0 };
        p1.PlayArea.Add(ronin);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = ronin };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context));
    }
}
