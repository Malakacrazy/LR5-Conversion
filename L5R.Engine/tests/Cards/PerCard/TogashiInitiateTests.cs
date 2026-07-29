using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class TogashiInitiateTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "togashi-initiate.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void WhileAttacking_PayingFateToAnUnclaimedRingHonorsItself()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var initiate = new Card { Id = "togashi-initiate", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(initiate);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(initiate);
        game.CurrentConflict = conflict;

        var voidRing = game.Rings.Single(r => r.Element == "void");

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = initiate, CostRingTarget = voidRing };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(p1.Fate, Is.EqualTo(1), "payFateToRing(1) cost was paid");
        Assert.That(voidRing.Fate, Is.EqualTo(1));
        Assert.That(initiate.IsHonored, Is.True);
    }

    [Test]
    public void CannotPayFateToAnAlreadyClaimedRing()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var initiate = new Card { Id = "togashi-initiate", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(initiate);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(initiate);
        game.CurrentConflict = conflict;

        var voidRing = game.Rings.Single(r => r.Element == "void");
        voidRing.Claimed = true;
        voidRing.ClaimedBy = p2;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = initiate, CostRingTarget = voidRing };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context));
        Assert.That(p1.Fate, Is.EqualTo(2), "the cost was never actually paid");
    }

    [Test]
    public void WhileNotAttacking_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var initiate = new Card { Id = "togashi-initiate", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(initiate);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = initiate };

        Assert.That(new AbilityExecutor(new CostRegistry(), new GameActionRegistry()).IsConditionMet(action, context), Is.False);
    }
}
