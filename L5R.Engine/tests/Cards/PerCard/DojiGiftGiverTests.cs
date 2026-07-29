using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class DojiGiftGiverTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "doji-gift-giver.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void WhileParticipating_GivingFateToTheOpponentBowsTheirParticipatingCharacter()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2", Fate = 1 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var giftGiver = new Card { Id = "doji-gift-giver", Type = CardType.Character, Controller = p1 };
        var enemy = new Card { Id = "enemy-defender", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(giftGiver);
        p2.PlayArea.Add(enemy);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(giftGiver);
        conflict.Defenders.Add(enemy);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = giftGiver };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: enemy);

        Assert.That(p1.Fate, Is.EqualTo(1), "giveFateToOpponent(1) cost was paid");
        Assert.That(p2.Fate, Is.EqualTo(2));
        Assert.That(enemy.Bowed, Is.True);
    }

    [Test]
    public void WhileNotParticipating_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var giftGiver = new Card { Id = "doji-gift-giver", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(giftGiver);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = giftGiver };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.That(executor.IsConditionMet(action, context), Is.False);
    }
}
