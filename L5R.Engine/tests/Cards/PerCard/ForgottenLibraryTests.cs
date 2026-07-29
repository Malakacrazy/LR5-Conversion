using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ForgottenLibraryTests
{
    [Test]
    public void AtTheStartOfTheDrawPhase_DrawsACard()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Draw };
        var library = new Card { Id = "forgotten-library", Type = CardType.Holding, Controller = p1 };
        p1.Deck.Add(new Card { Id = "top-card", Type = CardType.Character, Controller = p1 });

        var context = new AbilityContext { Game = game, Player = p1, Source = library };

        new ForgottenLibraryDrawOnDrawPhase().Execute(context);

        Assert.That(p1.Hand, Has.Count.EqualTo(1));
    }

    [Test]
    public void OutsideTheDrawPhase_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var library = new Card { Id = "forgotten-library", Type = CardType.Holding, Controller = p1 };

        var context = new AbilityContext { Game = game, Player = p1, Source = library };

        Assert.Throws<InvalidOperationException>(() => new ForgottenLibraryDrawOnDrawPhase().Execute(context));
    }
}
