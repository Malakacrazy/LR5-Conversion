using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class CallingInFavorsBotActionTests
{
    private static (GameState game, Card cif, Card costTarget, Card attachment) NewScenario()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var cif = new Card { Id = "calling-in-favors", Type = CardType.Event, Controller = p1 };
        var costTarget = new Card { Id = "my-character", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(costTarget);
        var parent = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        var attachment = new Card { Id = "opponent-attachment", Type = CardType.Attachment, Controller = p2, AttachedTo = parent };
        p2.PlayArea.Add(parent);
        p2.PlayArea.Add(attachment);
        return (game, cif, costTarget, attachment);
    }

    [Test]
    public void IsLegal_WithACostTargetAndAnOpponentAttachment_True()
    {
        var (game, cif, _, _) = NewScenario();
        Assert.That(new CallingInFavorsBotAction().IsLegal(game, cif, game.Player1), Is.True);
    }

    [Test]
    public void Invoke_DishonorsTheCostTargetAndTakesControlOfTheAttachment()
    {
        var (game, cif, costTarget, attachment) = NewScenario();
        new CallingInFavorsBotAction().Invoke(game, cif, game.Player1);

        Assert.That(costTarget.IsDishonored, Is.True);
        Assert.That(attachment.AttachedTo, Is.EqualTo(costTarget));
    }
}
