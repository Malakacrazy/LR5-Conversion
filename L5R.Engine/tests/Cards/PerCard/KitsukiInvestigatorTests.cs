using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class KitsukiInvestigatorTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "kitsuki-investigator.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    private static (GameState Game, Card Investigator, Card CardToDiscard, Card OtherHandCard) NewGameDuringAPoliticalConflict()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var investigator = new Card { Id = "kitsuki-investigator", Type = CardType.Character, Controller = p1 };
        var cardToDiscard = new Card { Id = "opponent-card-1", Type = CardType.Event, Controller = p2, Location = "hand" };
        var otherHandCard = new Card { Id = "opponent-card-2", Type = CardType.Event, Controller = p2, Location = "hand" };
        p1.PlayArea.Add(investigator);
        p2.Hand.Add(cardToDiscard);
        p2.Hand.Add(otherHandCard);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        conflict.Attackers.Add(investigator);
        game.CurrentConflict = conflict;

        return (game, investigator, cardToDiscard, otherHandCard);
    }

    [Test]
    public void PayingFateToAnUnclaimedRing_DiscardsTheChosenCardFromTheOpponentsHand()
    {
        var (game, investigator, cardToDiscard, otherHandCard) = NewGameDuringAPoliticalConflict();
        var voidRing = game.Rings.Single(r => r.Element == "void");

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = investigator, CostRingTarget = voidRing, ChosenCardMenuCard = cardToDiscard };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(game.Player1.Fate, Is.EqualTo(1), "payFateToRing(1)");
        Assert.That(voidRing.Fate, Is.EqualTo(1));
        Assert.That(game.Player2.Discard, Does.Contain(cardToDiscard));
        Assert.That(game.Player2.Hand, Does.Contain(otherHandCard), "only the chosen card is discarded");
    }

    [Test]
    public void CannotChooseACardOutsideTheOpponentsHand()
    {
        var (game, investigator, _, _) = NewGameDuringAPoliticalConflict();
        var voidRing = game.Rings.Single(r => r.Element == "void");
        var notInHand = new Card { Id = "not-in-hand", Type = CardType.Event, Controller = game.Player2 };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = investigator, CostRingTarget = voidRing, ChosenCardMenuCard = notInHand };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context));
    }

    [Test]
    public void WhenTheOpponentsHandIsEmpty_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var investigator = new Card { Id = "kitsuki-investigator", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(investigator);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        conflict.Attackers.Add(investigator);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = investigator };

        Assert.That(new AbilityExecutor(new CostRegistry(), new GameActionRegistry()).IsConditionMet(action, context), Is.False);
    }

    [Test]
    public void WhileNotParticipating_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var investigator = new Card { Id = "kitsuki-investigator", Type = CardType.Character, Controller = p1 };
        var handCard = new Card { Id = "opponent-card", Type = CardType.Event, Controller = p2, Location = "hand" };
        p1.PlayArea.Add(investigator);
        p2.Hand.Add(handCard);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = investigator };

        Assert.That(new AbilityExecutor(new CostRegistry(), new GameActionRegistry()).IsConditionMet(action, context), Is.False, "not participating in the conflict");
    }
}
