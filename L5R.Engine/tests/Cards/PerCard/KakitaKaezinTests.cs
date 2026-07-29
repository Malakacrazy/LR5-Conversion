using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class KakitaKaezinTests
{
    private static (GameState Game, Card Kaezin, Card Target, Card UninvolvedFriendly, Card UninvolvedOpponent) NewGameWithUninvolvedParticipants()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var kaezin = new Card { Id = "kakita-kaezin", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "duel-target", Type = CardType.Character, Controller = p2 };
        var uninvolvedFriendly = new Card { Id = "uninvolved-friendly", Type = CardType.Character, Controller = p1 };
        var uninvolvedOpponent = new Card { Id = "uninvolved-opponent", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(kaezin);
        p1.PlayArea.Add(uninvolvedFriendly);
        p2.PlayArea.Add(target);
        p2.PlayArea.Add(uninvolvedOpponent);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(kaezin);
        conflict.Attackers.Add(uninvolvedFriendly);
        conflict.Defenders.Add(target);
        conflict.Defenders.Add(uninvolvedOpponent);
        game.CurrentConflict = conflict;

        return (game, kaezin, target, uninvolvedFriendly, uninvolvedOpponent);
    }

    [Test]
    public void WhenKaezinWins_SendsEveryUninvolvedCharacterHome()
    {
        var (game, kaezin, target, uninvolvedFriendly, uninvolvedOpponent) = NewGameWithUninvolvedParticipants();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = kaezin, Target = target, DuelWinner = kaezin };

        new KakitaKaezinDuelAndSendHomeByOutcome().Execute(context);

        Assert.That(game.CurrentConflict!.Attackers, Does.Not.Contain(uninvolvedFriendly));
        Assert.That(game.CurrentConflict!.Defenders, Does.Not.Contain(uninvolvedOpponent));
        Assert.That(game.CurrentConflict!.Attackers, Does.Contain(kaezin), "the duelists stay");
        Assert.That(game.CurrentConflict!.Defenders, Does.Contain(target));
    }

    [Test]
    public void WhenKaezinLoses_SendsKaezinHomeInstead()
    {
        var (game, kaezin, target, uninvolvedFriendly, uninvolvedOpponent) = NewGameWithUninvolvedParticipants();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = kaezin, Target = target, DuelWinner = target };

        new KakitaKaezinDuelAndSendHomeByOutcome().Execute(context);

        Assert.That(game.CurrentConflict!.Attackers, Does.Not.Contain(kaezin));
        Assert.That(game.CurrentConflict!.Attackers, Does.Contain(uninvolvedFriendly), "uninvolved characters stay when kaezin loses");
        Assert.That(game.CurrentConflict!.Defenders, Does.Contain(uninvolvedOpponent));
    }
}
