using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class WatchCommanderFirerTests
{
    private static (Player p1, Player p2, GameState game, Card watchCommander, Card parent) NewScenario(int p1Honor = 5)
    {
        var p1 = new Player { Name = "Player1", Honor = p1Honor };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        var parent = new Card { Id = "parent", Type = CardType.Character, Controller = p2 };
        var watchCommander = new Card { Id = "watch-commander", Type = CardType.Attachment, Controller = p2, AttachedTo = parent };
        p2.PlayArea.Add(parent);
        p2.PlayArea.Add(watchCommander);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Defenders.Add(parent);
        game.CurrentConflict = conflict;

        return (p1, p2, game, watchCommander, parent);
    }

    [Test]
    public void FireEligibleReactions_WhenParentIsParticipating_MakesThePlayingPlayerLoseHonor()
    {
        var (p1, _, game, _, _) = NewScenario();

        WatchCommanderFirer.FireEligibleReactions(game, p1);

        Assert.That(p1.Honor, Is.EqualTo(4));
    }

    [Test]
    public void FireEligibleReactions_WhenParentIsNotParticipating_DoesNotFire()
    {
        var (p1, _, game, _, parent) = NewScenario();
        game.CurrentConflict!.Defenders.Remove(parent);

        WatchCommanderFirer.FireEligibleReactions(game, p1);

        Assert.That(p1.Honor, Is.EqualTo(5));
    }

    [Test]
    public void FireEligibleReactions_WithTwoCopiesBothParticipating_FiresBoth()
    {
        var (p1, p2, game, _, _) = NewScenario();
        var secondParent = new Card { Id = "second-parent", Type = CardType.Character, Controller = p2 };
        var secondWatchCommander = new Card { Id = "watch-commander", Type = CardType.Attachment, Controller = p2, AttachedTo = secondParent };
        p2.PlayArea.Add(secondParent);
        p2.PlayArea.Add(secondWatchCommander);
        game.CurrentConflict!.Defenders.Add(secondParent);

        WatchCommanderFirer.FireEligibleReactions(game, p1);

        Assert.That(p1.Honor, Is.EqualTo(3));
    }

    [Test]
    public void PlayCardGameActionHandler_PlayingACard_FiresTheOpponentsWatchCommander()
    {
        var (p1, _, game, _, _) = NewScenario();
        var playedCard = new Card { Id = "some-event", Type = CardType.Event, Controller = p1, Location = "hand", PrintedCost = 0 };
        p1.Hand.Add(playedCard);

        var context = new AbilityContext { Game = game, Player = p1, Source = playedCard };
        new PlayCardGameActionHandler().Execute(context, null);

        Assert.That(p1.Honor, Is.EqualTo(4));
    }
}
