using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class MirumotoRaitsuguTests
{
    private static (GameState Game, Card Raitsugu, Card Target) NewGameBothParticipating()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var raitsugu = new Card { Id = "mirumoto-raitsugu", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(raitsugu);
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(raitsugu);
        conflict.Defenders.Add(target);
        game.CurrentConflict = conflict;

        return (game, raitsugu, target);
    }

    [Test]
    public void WhenTheOpponentLosesWithFate_RemovesOneFate()
    {
        var (game, raitsugu, target) = NewGameBothParticipating();
        target.Fate = 2;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = raitsugu, Target = target, DuelWinner = raitsugu };

        new MirumotoRaitsuguDuelAndPunishLoser().Execute(context);

        Assert.That(target.Fate, Is.EqualTo(1));
        Assert.That(game.Player2.PlayArea, Does.Contain(target));
    }

    [Test]
    public void WhenTheOpponentLosesWithNoFate_IsDiscarded()
    {
        var (game, raitsugu, target) = NewGameBothParticipating();
        target.Fate = 0;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = raitsugu, Target = target, DuelWinner = raitsugu };

        new MirumotoRaitsuguDuelAndPunishLoser().Execute(context);

        Assert.That(game.Player2.Discard, Does.Contain(target));
    }

    [Test]
    public void WhenRaitsuguLoses_PunishesRaitsuguInstead()
    {
        var (game, raitsugu, target) = NewGameBothParticipating();
        raitsugu.Fate = 0;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = raitsugu, Target = target, DuelWinner = target };

        new MirumotoRaitsuguDuelAndPunishLoser().Execute(context);

        Assert.That(game.Player1.Discard, Does.Contain(raitsugu));
    }
}
