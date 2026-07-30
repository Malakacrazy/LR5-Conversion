using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class StandYourGroundTests
{
    [Test]
    public void DiscardsTheHonoredStatusInsteadOfLettingTheCharacterLeavePlay()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var standYourGround = new Card { Id = "stand-your-ground", Type = CardType.Event, Controller = p1 };
        var target = new Card { Id = "honored-character", Type = CardType.Character, Controller = p1, IsHonored = true };
        p1.PlayArea.Add(target);

        var context = new AbilityContext { Game = game, Player = p1, Source = standYourGround, Target = target };

        new StandYourGroundDiscardTokenInsteadOfLeavingPlay().Execute(context);

        Assert.That(target.IsHonored, Is.False);
        Assert.That(p1.PlayArea, Does.Contain(target), "stays in play instead of leaving");
    }

    [Test]
    public void WhenTheTargetIsNotHonored_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var standYourGround = new Card { Id = "stand-your-ground", Type = CardType.Event, Controller = p1 };
        var target = new Card { Id = "plain-character", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(target);

        var context = new AbilityContext { Game = game, Player = p1, Source = standYourGround, Target = target };

        Assert.Throws<InvalidOperationException>(() => new StandYourGroundDiscardTokenInsteadOfLeavingPlay().Execute(context));
    }

    [Test]
    public void WhenTheTargetIsControlledByTheOpponent_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var standYourGround = new Card { Id = "stand-your-ground", Type = CardType.Event, Controller = p1 };
        var target = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2, IsHonored = true };
        p2.PlayArea.Add(target);

        var context = new AbilityContext { Game = game, Player = p1, Source = standYourGround, Target = target };

        Assert.Throws<InvalidOperationException>(() => new StandYourGroundDiscardTokenInsteadOfLeavingPlay().Execute(context));
    }
}
