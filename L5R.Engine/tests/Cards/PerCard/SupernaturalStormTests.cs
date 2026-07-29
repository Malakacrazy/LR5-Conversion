using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SupernaturalStormTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "supernatural-storm.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void WithTwoShugenjaInPlay_GivesAParticipatingCharacterPlusTwoPlusTwo()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "supernatural-storm", Type = CardType.Event, Controller = p1 };
        var shugenja1 = new Card { Id = "shugenja-1", Type = CardType.Character, Controller = p1, Traits = new List<string> { "shugenja" } };
        var shugenja2 = new Card { Id = "shugenja-2", Type = CardType.Character, Controller = p1, Traits = new List<string> { "shugenja" } };
        var target = new Card { Id = "target-character", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 1, PrintedPoliticalSkill = 1 };
        p1.PlayArea.Add(shugenja1);
        p1.PlayArea.Add(shugenja2);
        p1.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(target);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);

        Assert.That(game.EffectiveMilitarySkill(target), Is.EqualTo(3));
        Assert.That(game.EffectivePoliticalSkill(target), Is.EqualTo(3));
    }

    [Test]
    public void WithNoShugenjaInPlay_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "supernatural-storm", Type = CardType.Event, Controller = p1 };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.That(executor.IsConditionMet(action, context), Is.False);
    }
}
