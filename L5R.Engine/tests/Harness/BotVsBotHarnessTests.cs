using L5R.Engine.GameSteps;
using L5R.Engine.Scheduling;

namespace L5R.Engine.Tests.Harness;

/// <summary>Proves roadmap step 11's literal exit bullet: a complete game between two trivial strategies runs end to end, no UI, without throwing or hanging.</summary>
public class BotVsBotHarnessTests
{
    [Test]
    public void TwoAlwaysPassBots_PlayACleanGameToTheRoundCapWithNoWinner()
    {
        var result = BotVsBotHarness.RunGame(seed: 1, new AlwaysPassBotPolicy(), new AlwaysPassBotPolicy(), roundCap: 5);

        Assert.That(result.FinalState, Is.EqualTo(StepState.Idle));
        Assert.That(result.Game.Winner, Is.Null, "two bots that never act or attack can't reach any win condition");
        Assert.That(result.Game.RoundNumber, Is.EqualTo(6), "stops the round once RoundNumber exceeds the cap of 5");
    }

    [Test]
    public void TwoFirstLegalActionBots_PlayACompleteGameEndToEndWithoutThrowing()
    {
        HarnessResult? result = null;
        Assert.DoesNotThrow(() => result = BotVsBotHarness.RunGame(seed: 42, new FirstLegalActionBotPolicy(), new FirstLegalActionBotPolicy(), roundCap: 20));

        Assert.That(result!.FinalState, Is.EqualTo(StepState.Idle));
        // A "complete game" is either a real recorded winner, or a clean round-cap exit -
        // never a hang (Blocked - nothing here ever waits on an unresolved prompt) or an
        // unhandled exception.
        var reachedCap = result.Game.RoundNumber > 20;
        Assert.That(result.Game.Winner is not null || reachedCap, Is.True);
    }

    [Test]
    public void AsymmetricPolicies_StillCompleteCleanly()
    {
        var result = BotVsBotHarness.RunGame(seed: 5, new FirstLegalActionBotPolicy(), new AlwaysPassBotPolicy(), roundCap: 30);

        Assert.That(result.FinalState, Is.EqualTo(StepState.Idle));
    }
}
