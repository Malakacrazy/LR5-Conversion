using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class BlackmailArtistBotActionTests
{
    private static GameState NewScenario(out Player p1, out Player p2, out Card artist)
    {
        p1 = new Player { Name = "Player1", Honor = 5 };
        p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        artist = new Card { Id = "blackmail-artist", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(artist);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political", Winner = p1 };
        conflict.Attackers.Add(artist);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_AfterWinningAPoliticalConflict_True()
    {
        var game = NewScenario(out var p1, out _, out var artist);

        Assert.That(new BlackmailArtistBotAction().IsLegal(game, artist, p1), Is.True);
    }

    [Test]
    public void IsLegal_AfterWinningAMilitaryConflict_False()
    {
        var game = NewScenario(out var p1, out _, out var artist);
        game.CurrentConflict!.ConflictType = "military";

        Assert.That(new BlackmailArtistBotAction().IsLegal(game, artist, p1), Is.False);
    }

    [Test]
    public void Invoke_TakesOneHonorFromTheOpponent()
    {
        var game = NewScenario(out var p1, out var p2, out var artist);

        new BlackmailArtistBotAction().Invoke(game, artist, p1);

        Assert.That(p1.Honor, Is.EqualTo(6));
        Assert.That(p2.Honor, Is.EqualTo(4));
    }
}
