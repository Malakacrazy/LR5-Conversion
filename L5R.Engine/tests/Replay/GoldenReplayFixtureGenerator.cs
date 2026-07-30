using L5R.Engine.Tests.Harness;

namespace L5R.Engine.Tests.Replay;

/// <summary>
/// Writes every GoldenReplayTests.Scenario's canonical event-log bytes to its fixture file
/// under Fixtures/. [Explicit] - excluded from the normal `dotnet test` run, same as any
/// golden-file generator: it always "passes" by construction (it just writes files), so
/// running it accidentally would silently paper over a real regression instead of catching
/// one. Run this deliberately, by name, only when a change to card/conflict/round-loop
/// behavior is intentional - never to make GoldenReplayTests pass after an accidental
/// regression. After running, review the diff on the regenerated fixture(s) before
/// committing, the same as any other golden file.
/// </summary>
[Explicit("Regenerates committed golden fixtures - run deliberately, review the diff, never to silence a real regression.")]
public class GoldenReplayFixtureGenerator
{
    [Test]
    public void RegenerateAllFixtures()
    {
        var fixturesDir = Path.Combine(AppContext.BaseDirectory, "Replay", "Fixtures");
        Directory.CreateDirectory(fixturesDir);

        foreach (var scenario in GoldenReplayTests.Scenarios)
        {
            var result = BotVsBotHarness.RunGame(scenario.Seed, scenario.Player1Policy(), scenario.Player2Policy(), scenario.RoundCap, scenario.Deck?.Invoke());
            File.WriteAllBytes(Path.Combine(fixturesDir, scenario.FixtureFileName), result.EventLog.ToCanonicalBytes());
        }
    }
}
