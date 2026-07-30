using L5R.Engine.State;

namespace L5R.Engine.Tests.State;

public class GameStateTests
{
    private static GameState NewGame(Phase startingPhase)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        return new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = startingPhase };
    }

    [Test]
    public void AdvancePhase_CyclesThroughTheRoundInOrder()
    {
        var game = NewGame(Phase.Dynasty);

        game.AdvancePhase();
        Assert.That(game.CurrentPhase, Is.EqualTo(Phase.Draw));

        game.AdvancePhase();
        Assert.That(game.CurrentPhase, Is.EqualTo(Phase.Conflict));

        game.AdvancePhase();
        Assert.That(game.CurrentPhase, Is.EqualTo(Phase.Fate));

        game.AdvancePhase();
        Assert.That(game.CurrentPhase, Is.EqualTo(Phase.Dynasty), "ringteki's beginRound() loops Fate back into a new Dynasty phase");
    }

    [Test]
    public void AdvancePhase_IncrementsRoundNumber_OnlyWhenReturningToDynasty()
    {
        var game = NewGame(Phase.Dynasty);
        Assert.That(game.RoundNumber, Is.EqualTo(1));

        game.AdvancePhase(); // -> Draw
        Assert.That(game.RoundNumber, Is.EqualTo(1));

        game.AdvancePhase(); // -> Conflict
        game.AdvancePhase(); // -> Fate
        Assert.That(game.RoundNumber, Is.EqualTo(1));

        game.AdvancePhase(); // -> Dynasty (round 2)
        Assert.That(game.RoundNumber, Is.EqualTo(2));
    }

    [Test]
    public void AdvancePhase_FlipsActivePlayer_OnlyOnTheDynastyRollover()
    {
        var game = NewGame(Phase.Dynasty);
        var p1 = game.ActivePlayer;
        var p2 = game.Opponent(p1);

        game.AdvancePhase(); // -> Draw
        Assert.That(game.ActivePlayer, Is.EqualTo(p1), "only the Dynasty rollover flips it, not every phase change");

        game.AdvancePhase(); // -> Conflict
        game.AdvancePhase(); // -> Fate
        Assert.That(game.ActivePlayer, Is.EqualTo(p1));

        game.AdvancePhase(); // -> Dynasty (round 2)
        Assert.That(game.ActivePlayer, Is.EqualTo(p2), "mirrors fatephase.js's unconditional passFirstPlayer() step");
    }

    [Test]
    public void AdvancePhase_KeepsActivePlayer_WhenFirstPlayerPassCancelled_AndConsumesTheFlag()
    {
        var game = NewGame(Phase.Fate);
        var p1 = game.ActivePlayer;
        var p2 = game.Opponent(p1);
        game.FirstPlayerPassCancelled = true;

        game.AdvancePhase(); // -> Dynasty (round 2), pass cancelled
        Assert.That(game.ActivePlayer, Is.EqualTo(p1), "way-of-the-unicorn kept the token");
        Assert.That(game.FirstPlayerPassCancelled, Is.False, "the flag only cancels one pass");

        game.AdvancePhase(); // -> Draw
        game.AdvancePhase(); // -> Conflict
        game.AdvancePhase(); // -> Fate
        game.AdvancePhase(); // -> Dynasty (round 3), nothing cancels this one
        Assert.That(game.ActivePlayer, Is.EqualTo(p2), "without the flag set again, the token passes normally");
    }

    [Test]
    public void AdvancePhase_ThrowsFromRegroup_SinceTheRealRoundLoopNeverReachesIt()
    {
        var game = NewGame(Phase.Regroup);

        // Phases.Regroup exists in ringteki's Constants.ts, but game.js's beginRound()
        // never actually queues a RegroupPhase - FatePhase's own steps (ready cards,
        // return rings) cover that ground instead. Throwing here documents that gap
        // rather than silently picking an arbitrary "next" phase.
        Assert.Throws<NotSupportedException>(() => game.AdvancePhase());
    }

    [Test]
    public void EndConflict_ExpiresUntilEndOfConflictEffects_ButNotUntilEndOfPhaseOnes()
    {
        var game = NewGame(Phase.Conflict);
        var character = new Card { Id = "character", Type = CardType.Character, Controller = game.Player1, PrintedMilitarySkill = 2 };
        game.CurrentConflict = new Conflict { AttackingPlayer = game.Player1, DefendingPlayer = game.Player2 };
        game.LastingEffects.Add(new LastingEffect { Target = character, Stat = "military", Value = 2, Duration = "untilEndOfConflict" });
        game.LastingEffects.Add(new LastingEffect { Target = character, Stat = "military", Value = 5, Duration = "untilEndOfPhase" });

        game.EndConflict();

        Assert.That(game.CurrentConflict, Is.Null);
        Assert.That(game.EffectiveMilitarySkill(character), Is.EqualTo(7), "only the untilEndOfConflict effect (2) expired, the untilEndOfPhase one (5) survives the conflict ending");
    }
}
