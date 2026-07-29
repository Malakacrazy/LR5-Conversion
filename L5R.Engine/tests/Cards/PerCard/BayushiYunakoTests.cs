using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class BayushiYunakoTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "bayushi-yunako.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void WhileParticipating_SwitchesTheChosenCharactersMilitaryAndPoliticalSkill()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var yunako = new Card { Id = "bayushi-yunako", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "target", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 4, PrintedPoliticalSkill = 1 };
        p1.PlayArea.Add(yunako);
        p1.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(yunako);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = yunako };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);

        Assert.That(game.EffectiveMilitarySkill(target), Is.EqualTo(1));
        Assert.That(game.EffectivePoliticalSkill(target), Is.EqualTo(4));
    }

    [Test]
    public void AdditiveBonusesStillApplyOnTopOfTheSwitchedBase()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var yunako = new Card { Id = "bayushi-yunako", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "target", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 4, PrintedPoliticalSkill = 1 };
        p1.PlayArea.Add(yunako);
        p1.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(yunako);
        game.CurrentConflict = conflict;
        game.LastingEffects.Add(new LastingEffect { Target = target, Stat = "military", Value = 2, Duration = "untilEndOfConflict" });

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = yunako };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);

        Assert.That(game.EffectiveMilitarySkill(target), Is.EqualTo(3), "1 (switched base) + 2 (additive bonus)");
    }

    [Test]
    public void ExpiresAtEndOfConflict()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var yunako = new Card { Id = "bayushi-yunako", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "target", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 4, PrintedPoliticalSkill = 1 };
        p1.PlayArea.Add(yunako);
        p1.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(yunako);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = yunako };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);
        game.EndConflict();

        Assert.That(game.EffectiveMilitarySkill(target), Is.EqualTo(4));
        Assert.That(game.EffectivePoliticalSkill(target), Is.EqualTo(1));
    }

    [Test]
    public void WhileNotParticipating_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var yunako = new Card { Id = "bayushi-yunako", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(yunako);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = yunako };

        Assert.That(new AbilityExecutor(new CostRegistry(), new GameActionRegistry()).IsConditionMet(action, context), Is.False);
    }
}
