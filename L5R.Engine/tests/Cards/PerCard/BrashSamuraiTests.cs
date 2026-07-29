using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class BrashSamuraiTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "brash-samurai.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void WhenTheOnlyAttacker_ItIsHonored()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var samurai = new Card { Id = "brash-samurai", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(samurai);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(samurai);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = samurai };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(samurai.IsHonored, Is.True);
    }

    [Test]
    public void CannotBeUsed_WhenAnotherCharacterIsAlsoAttacking()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var samurai = new Card { Id = "brash-samurai", Type = CardType.Character, Controller = p1 };
        var otherAttacker = new Card { Id = "other-attacker", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(samurai);
        p1.PlayArea.Add(otherAttacker);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(samurai);
        conflict.Attackers.Add(otherAttacker);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = samurai };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        // "own" resolves to whichever role brash-samurai's controller currently holds
        // (attacker here) - the count must be exactly 1, not just "isParticipating".
        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context));
        Assert.That(samurai.IsHonored, Is.False);
    }
}
