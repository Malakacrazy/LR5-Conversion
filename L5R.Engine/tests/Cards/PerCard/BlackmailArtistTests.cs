using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class BlackmailArtistTests
{
    private static (GameState Game, Card BlackmailArtist) NewGameWonPoliticalConflict()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var blackmailArtist = new Card { Id = "blackmail-artist", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(blackmailArtist);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political", Winner = p1 };
        conflict.Attackers.Add(blackmailArtist);
        game.CurrentConflict = conflict;

        return (game, blackmailArtist);
    }

    [Test]
    public void AfterWinningAPoliticalConflict_TakesOneHonorFromTheOpponent()
    {
        var (game, blackmailArtist) = NewGameWonPoliticalConflict();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = blackmailArtist };

        new BlackmailArtistTakeHonorOnPoliticalWin().Execute(context);

        Assert.That(game.Player1.Honor, Is.EqualTo(4));
        Assert.That(game.Player2.Honor, Is.EqualTo(4));
    }

    [Test]
    public void AfterWinningAMilitaryConflict_Throws()
    {
        var (game, blackmailArtist) = NewGameWonPoliticalConflict();
        game.CurrentConflict!.ConflictType = "military";

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = blackmailArtist };

        Assert.Throws<InvalidOperationException>(() => new BlackmailArtistTakeHonorOnPoliticalWin().Execute(context));
    }

    [Test]
    public void WhenTheControllerDoesNotWin_Throws()
    {
        var (game, blackmailArtist) = NewGameWonPoliticalConflict();
        game.CurrentConflict!.Winner = game.Player2;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = blackmailArtist };

        Assert.Throws<InvalidOperationException>(() => new BlackmailArtistTakeHonorOnPoliticalWin().Execute(context));
    }

    [Test]
    public void WhileNotParticipating_Throws()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var blackmailArtist = new Card { Id = "blackmail-artist", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(blackmailArtist);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political", Winner = p1 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = blackmailArtist };

        Assert.Throws<InvalidOperationException>(() => new BlackmailArtistTakeHonorOnPoliticalWin().Execute(context));
    }
}
