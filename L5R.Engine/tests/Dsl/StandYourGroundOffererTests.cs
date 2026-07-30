using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Cards;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class StandYourGroundOffererTests
{
    private static (Player p1, Card target, Card standYourGround) NewScenario()
    {
        var p1 = new Player { Name = "Player1" };
        var target = new Card { Id = "honored-character", Type = CardType.Character, Controller = p1, IsHonored = true };
        p1.PlayArea.Add(target);
        var standYourGround = new Card { Id = "stand-your-ground", Type = CardType.Event, Controller = p1, Location = "hand", PrintedCost = 0 };
        p1.Hand.Add(standYourGround);
        return (p1, target, standYourGround);
    }

    [Test]
    public void TryInterrupt_WithAnHonoredTargetAndCardInHand_DiscardsTheTokenInstead()
    {
        var (p1, target, standYourGround) = NewScenario();
        var game = new GameState { Player1 = p1, Player2 = new Player { Name = "Player2" }, ActivePlayer = p1 };

        var result = StandYourGroundOfferer.TryInterrupt(game, target);

        Assert.That(result, Is.True);
        Assert.That(target.IsHonored, Is.False);
        Assert.That(p1.PlayArea, Contains.Item(target), "the character never left play");
        Assert.That(p1.Hand, Does.Not.Contain(standYourGround));
        Assert.That(p1.Discard, Contains.Item(standYourGround));
    }

    [Test]
    public void TryInterrupt_WhenTheTargetIsNotHonored_ReturnsFalse()
    {
        var (p1, target, _) = NewScenario();
        target.IsHonored = false;
        var game = new GameState { Player1 = p1, Player2 = new Player { Name = "Player2" }, ActivePlayer = p1 };

        Assert.That(StandYourGroundOfferer.TryInterrupt(game, target), Is.False);
    }

    [Test]
    public void DiscardFromPlayGameActionHandler_DiscardingAnHonoredCharacterWithStandYourGroundInHand_KeepsItInPlay()
    {
        var (p1, target, standYourGround) = NewScenario();
        var game = new GameState { Player1 = p1, Player2 = new Player { Name = "Player2" }, ActivePlayer = p1 };

        var context = new AbilityContext { Game = game, Player = p1, Source = target, Target = target };
        new DiscardFromPlayGameActionHandler().Execute(context, null);

        Assert.That(p1.PlayArea, Contains.Item(target));
        Assert.That(target.IsHonored, Is.False);
        Assert.That(p1.Discard, Contains.Item(standYourGround));
    }

    [Test]
    public void IsNeverOfferedAsAnOrdinaryHandPlay()
    {
        // Regression coverage for a real bug found while adopting breakthrough - see
        // WouldInterruptOffererTests' own equivalent test for the full explanation.
        var p1 = new Player { Name = "Player1", Fate = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "stand-your-ground.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var card = CardFactory.BuildCard(document.RootElement, p1);
        card.Location = "hand";
        p1.Hand.Add(card);

        var legalPlays = LegalActions.GetLegalPlays(game, p1, "hand");

        Assert.That(legalPlays, Does.Not.Contain(card));
    }
}
