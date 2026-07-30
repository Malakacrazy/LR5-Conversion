using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class KitsuSpiritcallerBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card spiritcaller, out Card discardedCharacter)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        spiritcaller = new Card { Id = "kitsu-spiritcaller", Type = CardType.Character, Controller = p1 };
        discardedCharacter = new Card { Id = "discarded-character", Type = CardType.Character, Controller = p1, Location = "discard" };
        p1.PlayArea.Add(spiritcaller);
        p1.Discard.Add(discardedCharacter);

        game.CurrentConflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };

        return game;
    }

    [Test]
    public void IsLegal_DuringAConflictWithADiscardedCharacter_True()
    {
        var game = NewScenario(out var p1, out var spiritcaller, out _);

        Assert.That(new KitsuSpiritcallerBotAction().IsLegal(game, spiritcaller, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenAlreadyBowed_False()
    {
        var game = NewScenario(out var p1, out var spiritcaller, out _);
        spiritcaller.Bowed = true;

        Assert.That(new KitsuSpiritcallerBotAction().IsLegal(game, spiritcaller, p1), Is.False);
    }

    [Test]
    public void Invoke_BowsItselfAndPutsTheDiscardedCharacterIntoTheConflict()
    {
        var game = NewScenario(out var p1, out var spiritcaller, out var discardedCharacter);

        new KitsuSpiritcallerBotAction().Invoke(game, spiritcaller, p1);

        Assert.That(spiritcaller.Bowed, Is.True);
        Assert.That(p1.Discard, Does.Not.Contain(discardedCharacter));
        Assert.That(game.CurrentConflict!.Attackers, Contains.Item(discardedCharacter));
        Assert.That(game.EndOfConflictReturns, Contains.Item(discardedCharacter));
    }
}
