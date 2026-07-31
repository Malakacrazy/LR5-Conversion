using L5R.Engine.GameSteps;
using L5R.Engine.State;
using L5R.Engine.Tests.Harness;

namespace L5R.Engine.Tests.Replay;

/// <summary>
/// One fixed scenario for the golden replay suite - everything needed to reproduce a game
/// deterministically, plus which committed fixture file it must match. Deck defaults to null
/// (BotVsBotHarness.RunGame's own default, FixedDeck) so the original 4 scenarios don't need
/// to name it explicitly.
/// </summary>
public sealed record GoldenScenario(string Name, ulong Seed, Func<IBotPolicy> Player1Policy, Func<IBotPolicy> Player2Policy, int RoundCap, Func<DeckList>? Deck = null)
{
    public string FixtureFileName => $"{Name}.log";

    public override string ToString() => Name;
}

/// <summary>
/// Roadmap step 10: record the event stream of 3-5 complete simulated games via the headless
/// harness (BotVsBotHarness), and regression-test that replaying the same seed still produces
/// a byte-identical stream. This is the CI mechanism meant to catch any unintended behavior
/// change in later phases - a diff here means something about card resolution, conflict
/// resolution, or the round loop changed, even if every other test still passes.
///
/// The fixtures under Fixtures/*.log were generated once by GoldenReplayFixtureGenerator (an
/// [Explicit] one-shot test, not part of the normal run) and are committed to the repo like
/// any other golden-file test. Regenerate them deliberately (rerun that generator) only when
/// a change to card/conflict/round-loop behavior is intentional - never to make this test pass
/// after an accidental regression.
/// </summary>
public class GoldenReplayTests
{
    private static readonly string FixturesDir = Path.Combine(AppContext.BaseDirectory, "Replay", "Fixtures");

    public static readonly IReadOnlyList<GoldenScenario> Scenarios = new[]
    {
        new GoldenScenario("first-legal-vs-first-legal-101", 101, () => new FirstLegalActionBotPolicy(), () => new FirstLegalActionBotPolicy(), 15),
        new GoldenScenario("first-legal-vs-first-legal-202", 202, () => new FirstLegalActionBotPolicy(), () => new FirstLegalActionBotPolicy(), 15),
        new GoldenScenario("first-legal-vs-always-pass-303", 303, () => new FirstLegalActionBotPolicy(), () => new AlwaysPassBotPolicy(), 15),
        new GoldenScenario("always-pass-vs-always-pass-404", 404, () => new AlwaysPassBotPolicy(), () => new AlwaysPassBotPolicy(), 5),
        new GoldenScenario("scriptoverride-rich-vs-rich-505", 505, () => new FirstLegalActionBotPolicy(new ScriptedActionRegistry()), () => new FirstLegalActionBotPolicy(new ScriptedActionRegistry()), 20, () => BotVsBotHarness.RichDeck()),
    };

    [TestCaseSource(nameof(Scenarios))]
    public void ReplayMatchesTheCommittedGoldenFixture(GoldenScenario scenario)
    {
        var expected = File.ReadAllBytes(Path.Combine(FixturesDir, scenario.FixtureFileName));

        var result = BotVsBotHarness.RunGame(scenario.Seed, scenario.Player1Policy(), scenario.Player2Policy(), scenario.RoundCap, scenario.Deck?.Invoke());
        var actual = result.EventLog.ToCanonicalBytes();

        Assert.That(actual, Is.EqualTo(expected),
            $"Event log for scenario '{scenario.Name}' diverged from its committed golden fixture - " +
            "this means some card/conflict/round-loop behavior changed. If intentional, regenerate " +
            "the fixtures via GoldenReplayFixtureGenerator; if not, this caught a real regression.");
    }
}
