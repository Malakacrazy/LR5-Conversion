using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class CallingInFavorsTests
{
    [Test]
    public void DishonorsTheCostTargetAndTakesControlOfTheAttachment()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var callingInFavors = new Card { Id = "calling-in-favors", Type = CardType.Event, Controller = p1 };
        var costTarget = new Card { Id = "dishonor-fodder", Type = CardType.Character, Controller = p1 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        var attachment = new Card { Id = "opponent-attachment", Type = CardType.Attachment, Controller = p2, AttachedTo = opponentCharacter };
        p1.PlayArea.Add(costTarget);
        p2.PlayArea.Add(opponentCharacter);
        p2.PlayArea.Add(attachment);

        var context = new AbilityContext { Game = game, Player = p1, Source = callingInFavors, Target = attachment, CostTarget = costTarget };

        new CallingInFavorsAttachOrDiscard().Execute(context);

        Assert.That(costTarget.IsDishonored, Is.True);
        Assert.That(attachment.AttachedTo, Is.EqualTo(costTarget));
        Assert.That(attachment.Controller, Is.EqualTo(p1));
        Assert.That(p1.PlayArea, Does.Contain(attachment));
        Assert.That(p2.PlayArea, Does.Not.Contain(attachment));
    }

    [Test]
    public void WhenTargetingAFriendlyAttachment_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var callingInFavors = new Card { Id = "calling-in-favors", Type = CardType.Event, Controller = p1 };
        var costTarget = new Card { Id = "dishonor-fodder", Type = CardType.Character, Controller = p1 };
        var myCharacter = new Card { Id = "my-character", Type = CardType.Character, Controller = p1 };
        var myAttachment = new Card { Id = "my-attachment", Type = CardType.Attachment, Controller = p1, AttachedTo = myCharacter };
        p1.PlayArea.Add(costTarget);
        p1.PlayArea.Add(myCharacter);
        p1.PlayArea.Add(myAttachment);

        var context = new AbilityContext { Game = game, Player = p1, Source = callingInFavors, Target = myAttachment, CostTarget = costTarget };

        Assert.Throws<InvalidOperationException>(() => new CallingInFavorsAttachOrDiscard().Execute(context));
    }

    [Test]
    public void WhenTheCostTargetIsAlreadyDishonored_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var callingInFavors = new Card { Id = "calling-in-favors", Type = CardType.Event, Controller = p1 };
        var costTarget = new Card { Id = "dishonor-fodder", Type = CardType.Character, Controller = p1, IsDishonored = true };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        var attachment = new Card { Id = "opponent-attachment", Type = CardType.Attachment, Controller = p2, AttachedTo = opponentCharacter };
        p1.PlayArea.Add(costTarget);
        p2.PlayArea.Add(opponentCharacter);
        p2.PlayArea.Add(attachment);

        var context = new AbilityContext { Game = game, Player = p1, Source = callingInFavors, Target = attachment, CostTarget = costTarget };

        Assert.Throws<InvalidOperationException>(() => new CallingInFavorsAttachOrDiscard().Execute(context));
    }
}
