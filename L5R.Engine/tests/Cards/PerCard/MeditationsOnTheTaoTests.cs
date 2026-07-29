using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class MeditationsOnTheTaoTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "meditations-on-the-tao.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void RemovesOneFateFromAnAttackingCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var province = new Card { Id = "meditations-on-the-tao", Type = CardType.Province, Controller = p1 };
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p2, Fate = 2 };
        p2.PlayArea.Add(attacker);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Attackers.Add(attacker);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = province };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: attacker);

        Assert.That(attacker.Fate, Is.EqualTo(1));
    }

    [Test]
    public void CannotTargetANonAttackingCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var defender = new Card { Id = "defender", Type = CardType.Character, Controller = p1, Fate = 2 };
        p1.PlayArea.Add(defender);

        var action = LoadFirstAction();
        var legalTargets = TargetResolver.ResolveLegalTargets(
            action.Target!,
            new AbilityContext { Game = game, Player = p1, Source = new Card { Id = "meditations-on-the-tao", Type = CardType.Province, Controller = p1 } });

        Assert.That(legalTargets, Does.Not.Contain(defender));
    }
}
