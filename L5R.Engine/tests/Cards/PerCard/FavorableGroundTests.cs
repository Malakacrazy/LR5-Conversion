using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class FavorableGroundTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "favorable-ground.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void SendsAParticipatingCharacterHome_ThroughMoveToConflictIsIneligible()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var holding = new Card { Id = "favorable-ground", Type = CardType.Holding, Controller = p1 };
        var target = new Card { Id = "own-character", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(holding);
        p1.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(target);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = holding };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);

        Assert.That(p1.Discard, Does.Contain(holding), "sacrificeSelf cost was paid");
        Assert.That(conflict.Attackers, Does.Not.Contain(target), "already participating, so sendHome (not moveToConflict) applies");
    }

    [Test]
    public void MovesANonParticipatingCharacterIntoTheConflict_ThroughSendHomeIsIneligible()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var holding = new Card { Id = "favorable-ground", Type = CardType.Holding, Controller = p1 };
        var target = new Card { Id = "own-character", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(holding);
        p1.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = holding };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);

        Assert.That(conflict.Attackers, Does.Contain(target), "not yet participating, so moveToConflict (not sendHome) applies");
    }
}
