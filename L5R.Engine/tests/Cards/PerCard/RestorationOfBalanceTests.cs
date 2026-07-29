using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class RestorationOfBalanceTests
{
    private static TriggeredAbilityDefinition LoadFirstTriggeredAbility()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "restoration-of-balance.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseTriggeredAbilities(document.RootElement).Single();
    }

    [Test]
    public void WhenBroken_ForcesTheOpponentToDiscardDownToFourCards()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2, CurrentPhase = Phase.Conflict };
        var province = new Card { Id = "restoration-of-balance", Type = CardType.Province, Controller = p1 };

        var hand = Enumerable.Range(1, 6).Select(i => new Card { Id = $"hand-{i}", Type = CardType.Character, Controller = p2, Location = "hand" }).ToList();
        foreach (var card in hand)
            p2.Hand.Add(card);

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext
        {
            Game = game, Player = p1, Source = province,
            ChosenDiscardCards = new[] { hand[0], hand[1] }
        };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.ExecuteTriggered(ability, context, eventCard: province);

        Assert.That(p2.Discard, Does.Contain(hand[0]));
        Assert.That(p2.Discard, Does.Contain(hand[1]));
        Assert.That(p2.Hand, Has.Count.EqualTo(4), "6 cards minus 2 discarded leaves exactly the 4-card hand size limit");
    }

    [Test]
    public void WithFourOrFewerCardsInHand_NoDiscardIsRequired()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2, CurrentPhase = Phase.Conflict };
        var province = new Card { Id = "restoration-of-balance", Type = CardType.Province, Controller = p1 };
        var handCard = new Card { Id = "hand-1", Type = CardType.Character, Controller = p2, Location = "hand" };
        p2.Hand.Add(handCard);

        var ability = LoadFirstTriggeredAbility();
        var context = new AbilityContext { Game = game, Player = p1, Source = province };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.ExecuteTriggered(ability, context, eventCard: province);

        Assert.That(p2.Hand, Does.Contain(handCard), "amount resolves to 0 (1 - 4, floored) so nothing is discarded and no ChosenDiscardCards was even needed");
    }
}
