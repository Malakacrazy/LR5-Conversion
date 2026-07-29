using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class MasterOfTheSpearTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "master-of-the-spear.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void WhileAttacking_SendsAParticipatingOpponentCharacterHome()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var master = new Card { Id = "master-of-the-spear", Type = CardType.Character, Controller = p1 };
        var opponentDefender = new Card { Id = "opponent-defender", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(master);
        p2.PlayArea.Add(opponentDefender);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(master);
        conflict.Defenders.Add(opponentDefender);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = master };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: opponentDefender);

        Assert.That(conflict.Defenders, Does.Not.Contain(opponentDefender));
        Assert.That(conflict.Attackers, Does.Not.Contain(opponentDefender));
    }
}
