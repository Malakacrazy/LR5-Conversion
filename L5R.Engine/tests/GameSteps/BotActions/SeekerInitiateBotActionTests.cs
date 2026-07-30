using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class SeekerInitiateBotActionTests
{
    private static (GameState game, Card seekerInitiate) NewScenario()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        p1.Role = new Card { Id = "seeker-of-water", Type = CardType.Role, Controller = p1, Traits = new[] { "water" } };

        var seekerInitiate = new Card { Id = "seeker-initiate", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(seekerInitiate);
        p1.Deck.Add(new Card { Id = "deck-card", Type = CardType.Character, Controller = p1 });

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, RingClaimedThisConflict = true };
        conflict.Elements.Add("water");
        game.CurrentConflict = conflict;

        var ring = game.Rings.Find(r => r.Element == "water")!;
        ring.Claimed = true;
        ring.ClaimedBy = p1;

        return (game, seekerInitiate);
    }

    [Test]
    public void IsLegal_AfterClaimingTheMatchingRing_True()
    {
        var (game, seekerInitiate) = NewScenario();
        Assert.That(new SeekerInitiateBotAction().IsLegal(game, seekerInitiate, game.Player1), Is.True);
    }

    [Test]
    public void IsLegal_WhenTheRoleDoesNotMatch_False()
    {
        var (game, seekerInitiate) = NewScenario();
        game.Player1.Role = new Card { Id = "seeker-of-fire", Type = CardType.Role, Controller = game.Player1, Traits = new[] { "fire" } };

        Assert.That(new SeekerInitiateBotAction().IsLegal(game, seekerInitiate, game.Player1), Is.False);
    }

    [Test]
    public void Invoke_TakesTheTopCardOfTheDeck()
    {
        var (game, seekerInitiate) = NewScenario();
        var topCard = game.Player1.Deck[0];

        new SeekerInitiateBotAction().Invoke(game, seekerInitiate, game.Player1);

        Assert.That(game.Player1.Hand, Contains.Item(topCard));
    }
}
