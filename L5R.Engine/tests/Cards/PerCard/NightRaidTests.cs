using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class NightRaidTests
{
    private static TriggeredAbilityDefinition LoadFirstTriggeredAbility()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "night-raid.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseTriggeredAbilities(document.RootElement).Single();
    }

    [Test]
    public void WhenRevealed_ForcesTheAttackerToDiscardCardsEqualToTheAttackerCount()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2, CurrentPhase = Phase.Conflict };
        var province = new Card { Id = "night-raid", Type = CardType.Province, Controller = p1 };
        var attacker1 = new Card { Id = "attacker-1", Type = CardType.Character, Controller = p2 };
        var attacker2 = new Card { Id = "attacker-2", Type = CardType.Character, Controller = p2 };
        var handCard1 = new Card { Id = "hand-1", Type = CardType.Character, Controller = p2, Location = "hand" };
        var handCard2 = new Card { Id = "hand-2", Type = CardType.Character, Controller = p2, Location = "hand" };
        var handCard3 = new Card { Id = "hand-3", Type = CardType.Character, Controller = p2, Location = "hand" };
        p2.Hand.Add(handCard1);
        p2.Hand.Add(handCard2);
        p2.Hand.Add(handCard3);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Attackers.Add(attacker1);
        conflict.Attackers.Add(attacker2);
        game.CurrentConflict = conflict;

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext
        {
            Game = game, Player = p1, Source = province,
            ChosenDiscardCards = new[] { handCard1, handCard2 }
        };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.ExecuteTriggered(ability, context, eventCard: province);

        Assert.That(p2.Discard, Does.Contain(handCard1));
        Assert.That(p2.Discard, Does.Contain(handCard2));
        Assert.That(p2.Hand, Does.Contain(handCard3), "only 2 cards (the attacker count) are discarded");
    }

    [Test]
    public void WithFewerHandCardsThanAttackers_OnlyDiscardsWhatsInHand()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2, CurrentPhase = Phase.Conflict };
        var province = new Card { Id = "night-raid", Type = CardType.Province, Controller = p1 };
        var attacker1 = new Card { Id = "attacker-1", Type = CardType.Character, Controller = p2 };
        var attacker2 = new Card { Id = "attacker-2", Type = CardType.Character, Controller = p2 };
        var handCard1 = new Card { Id = "hand-1", Type = CardType.Character, Controller = p2, Location = "hand" };
        p2.Hand.Add(handCard1);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Attackers.Add(attacker1);
        conflict.Attackers.Add(attacker2);
        game.CurrentConflict = conflict;

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext
        {
            Game = game, Player = p1, Source = province,
            ChosenDiscardCards = new[] { handCard1 }
        };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.ExecuteTriggered(ability, context, eventCard: province);

        Assert.That(p2.Discard, Does.Contain(handCard1));
        Assert.That(p2.Hand, Is.Empty);
    }
}
