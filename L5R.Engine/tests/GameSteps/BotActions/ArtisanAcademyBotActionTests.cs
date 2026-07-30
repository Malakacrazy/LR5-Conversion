using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class ArtisanAcademyBotActionTests
{
    [Test]
    public void IsLegal_DuringConflictPhaseWithCardsInDeck_True()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        p1.Deck.Add(new Card { Id = "top-card", Type = CardType.Character, Controller = p1 });
        var academy = new Card { Id = "artisan-academy", Type = CardType.Holding, Controller = p1 };

        Assert.That(new ArtisanAcademyBotAction().IsLegal(game, academy, p1), Is.True);
    }

    [Test]
    public void IsLegal_WithAnEmptyDeck_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var academy = new Card { Id = "artisan-academy", Type = CardType.Holding, Controller = p1 };

        Assert.That(new ArtisanAcademyBotAction().IsLegal(game, academy, p1), Is.False);
    }

    [Test]
    public void Invoke_DoesNotThrow()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        p1.Deck.Add(new Card { Id = "top-card", Type = CardType.Character, Controller = p1 });
        var academy = new Card { Id = "artisan-academy", Type = CardType.Holding, Controller = p1 };

        Assert.DoesNotThrow(() => new ArtisanAcademyBotAction().Invoke(game, academy, p1));
    }
}
