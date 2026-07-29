using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class WarriorPoetTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "warrior-poet.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void WhileParticipating_GivesEveryOpposingParticipantMinusOneMinusOne()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var poet = new Card { Id = "warrior-poet", Type = CardType.Character, Controller = p1 };
        var enemy = new Card { Id = "enemy-1", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 2, PrintedPoliticalSkill = 2 };
        var bystander = new Card { Id = "bystander", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 2, PrintedPoliticalSkill = 2 };
        p1.PlayArea.Add(poet);
        p2.PlayArea.Add(enemy);
        p2.PlayArea.Add(bystander);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(poet);
        conflict.Defenders.Add(enemy);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = poet };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(game.EffectiveMilitarySkill(enemy), Is.EqualTo(1));
        Assert.That(game.EffectivePoliticalSkill(enemy), Is.EqualTo(1));
        Assert.That(game.EffectiveMilitarySkill(bystander), Is.EqualTo(2), "not participating, so not affected");
    }

    [Test]
    public void WhileNotParticipating_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var poet = new Card { Id = "warrior-poet", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(poet);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = poet };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.That(executor.IsConditionMet(action, context), Is.False);
    }
}
