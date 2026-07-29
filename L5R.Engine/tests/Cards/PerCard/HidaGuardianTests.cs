using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class HidaGuardianTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "hida-guardian.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void WhileParticipating_GivesAnotherParticipantABonusPerHoldingControlled()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var guardian = new Card { Id = "hida-guardian", Type = CardType.Character, Controller = p1 };
        var ally = new Card { Id = "ally", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 1, PrintedPoliticalSkill = 1 };
        var holding1 = new Card { Id = "holding-1", Type = CardType.Holding, Controller = p1 };
        var holding2 = new Card { Id = "holding-2", Type = CardType.Holding, Controller = p1 };
        p1.PlayArea.Add(guardian);
        p1.PlayArea.Add(ally);
        p1.PlayArea.Add(holding1);
        p1.PlayArea.Add(holding2);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(guardian);
        conflict.Attackers.Add(ally);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = guardian };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: ally);

        Assert.That(game.EffectiveMilitarySkill(ally), Is.EqualTo(5), "1 printed + (2 holdings * multiplier 2)");
        Assert.That(game.EffectivePoliticalSkill(ally), Is.EqualTo(5));
    }

    [Test]
    public void ItselfIsNotALegalTarget()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var guardian = new Card { Id = "hida-guardian", Type = CardType.Character, Controller = p1 };
        var ally = new Card { Id = "ally", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(guardian);
        p1.PlayArea.Add(ally);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(guardian);
        conflict.Attackers.Add(ally);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = guardian };
        var legalTargets = TargetResolver.ResolveLegalTargets(action.Target!, context);

        Assert.That(legalTargets, Does.Not.Contain(guardian), "cardCondition excludes isSelf");
        Assert.That(legalTargets, Does.Contain(ally));
    }

    [Test]
    public void WhileNotParticipating_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var guardian = new Card { Id = "hida-guardian", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(guardian);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = guardian };

        Assert.That(new AbilityExecutor(new CostRegistry(), new GameActionRegistry()).IsConditionMet(action, context), Is.False);
    }
}
