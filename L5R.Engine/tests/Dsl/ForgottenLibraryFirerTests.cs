using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class ForgottenLibraryFirerTests
{
    [Test]
    public void FireIfLegal_InPlayDuringTheDrawPhase_DrawsACard()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Draw };
        var library = new Card { Id = "forgotten-library", Type = CardType.Holding, Controller = p1 };
        p1.PlayArea.Add(library);
        p1.Deck.Add(new Card { Id = "top-card", Type = CardType.Character, Controller = p1 });

        ForgottenLibraryFirer.FireIfLegal(game, p1);

        Assert.That(p1.Hand, Has.Count.EqualTo(1));
    }

    [Test]
    public void FireIfLegal_WhenNotInPlay_DoesNothing()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Draw };
        p1.Deck.Add(new Card { Id = "top-card", Type = CardType.Character, Controller = p1 });

        ForgottenLibraryFirer.FireIfLegal(game, p1);

        Assert.That(p1.Hand, Is.Empty);
    }
}
