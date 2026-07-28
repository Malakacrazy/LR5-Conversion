using L5R.Engine.Logging;
using L5R.Engine.Randomness;

namespace L5R.Engine.Tests.Replay;

/// <summary>
/// The roadmap-mandated replay test: the same seed run twice must produce byte-identical
/// event logs. This is the actual guarantee replay, spectating, and reconnection depend
/// on - not just "the RNG is deterministic" or "the log is serializable" in isolation,
/// but that a full match setup driven by both together reproduces exactly.
/// </summary>
public class ReplayDeterminismTests
{
    [Test]
    public void SameSeed_ProducesByteIdenticalEventLogs()
    {
        var firstRun = SimulateMatchSetup(seed: 20260728);
        var secondRun = SimulateMatchSetup(seed: 20260728);

        Assert.That(secondRun, Is.EqualTo(firstRun));
    }

    [Test]
    public void DifferentSeeds_ProduceDifferentEventLogs()
    {
        // Guards against the test above passing for the wrong reason (e.g. everything
        // in SimulateMatchSetup being accidentally constant).
        var a = SimulateMatchSetup(seed: 1);
        var b = SimulateMatchSetup(seed: 2);

        Assert.That(a, Is.Not.EqualTo(b));
    }

    /// <summary>
    /// Stands in for match setup until real Game/Player/Deck types exist (task 8/9):
    /// shuffle a deck and roll each player's honor dial, logging every random decision -
    /// the same shape of work the real setup phase will do.
    /// </summary>
    private static byte[] SimulateMatchSetup(ulong seed)
    {
        var rng = new SeededRandom(seed);
        var log = new EventLog();

        var conflictDeck = Enumerable.Range(1, 40).Select(i => $"card-{i}").ToList();
        rng.Shuffle(conflictDeck);
        log.Record("onDeckShuffled", new Dictionary<string, string>
        {
            ["deck"] = "conflict",
            ["order"] = string.Join(",", conflictDeck)
        });

        for (int player = 0; player < 2; player++)
        {
            var honorDial = rng.Next(6) + 1;
            log.Record("onHonorDialsRevealed", new Dictionary<string, string>
            {
                ["player"] = player.ToString(),
                ["value"] = honorDial.ToString()
            });
        }

        return log.ToCanonicalBytes();
    }
}
