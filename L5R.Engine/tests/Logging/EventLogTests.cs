using L5R.Engine.Logging;

namespace L5R.Engine.Tests.Logging;

public class EventLogTests
{
    [Test]
    public void Record_AssignsSequentialSequenceNumbers()
    {
        var log = new EventLog();

        log.Record("onPhaseStarted");
        log.Record("onCardPlayed");
        log.Record("onPhaseEnded");

        Assert.That(log.Entries.Select(e => e.Sequence), Is.EqualTo(new[] { 0, 1, 2 }));
    }

    [Test]
    public void ToCanonicalBytes_IsUnaffectedByDataKeyInsertionOrder()
    {
        // Dictionary iteration order happens to match insertion order in the current BCL,
        // but that's an implementation detail, not a contract - a replay comparison must
        // not depend on it. This pins down that ToCanonicalBytes sorts keys itself.
        var logA = new EventLog();
        logA.Record("onCardPlayed", new Dictionary<string, string> { ["card"] = "c1", ["player"] = "p1" });

        var logB = new EventLog();
        logB.Record("onCardPlayed", new Dictionary<string, string> { ["player"] = "p1", ["card"] = "c1" });

        Assert.That(logB.ToCanonicalBytes(), Is.EqualTo(logA.ToCanonicalBytes()));
    }

    [Test]
    public void ToCanonicalBytes_DiffersWhenEventDataDiffers()
    {
        var logA = new EventLog();
        logA.Record("onCardPlayed", new Dictionary<string, string> { ["card"] = "c1" });

        var logB = new EventLog();
        logB.Record("onCardPlayed", new Dictionary<string, string> { ["card"] = "c2" });

        Assert.That(logB.ToCanonicalBytes(), Is.Not.EqualTo(logA.ToCanonicalBytes()));
    }
}
