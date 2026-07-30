using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ReprieveTests
{
    [Test]
    public void DiscardsItselfInsteadOfTheParentLeavingPlay()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var parent = new Card { Id = "some-character", Type = CardType.Character, Controller = p1 };
        var reprieve = new Card { Id = "reprieve", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(reprieve);

        var context = new AbilityContext { Game = game, Player = p1, Source = reprieve };

        new ReprieveDiscardInsteadOfParentLeavingPlay().Execute(context);

        Assert.That(p1.Discard, Does.Contain(reprieve));
        Assert.That(p1.PlayArea, Does.Contain(parent), "the parent stays in play instead of leaving");
    }

    [Test]
    public void WhenNotAttachedToAnything_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var reprieve = new Card { Id = "reprieve", Type = CardType.Attachment, Controller = p1 };
        p1.PlayArea.Add(reprieve);

        var context = new AbilityContext { Game = game, Player = p1, Source = reprieve };

        Assert.Throws<InvalidOperationException>(() => new ReprieveDiscardInsteadOfParentLeavingPlay().Execute(context));
    }
}
