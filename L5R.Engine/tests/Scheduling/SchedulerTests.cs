using System.Collections;
using L5R.Engine.Scheduling;

namespace L5R.Engine.Tests.Scheduling;

/// <summary>
/// Spike tests for the scheduler port (ringteki gamepipeline.js). These gate card
/// porting: per the roadmap, no card work happens until nested interrupt resumption and
/// async-prompt resumption are proven correct here.
/// </summary>
public class SchedulerTests
{
    [Test]
    public void SimpleStep_RunsToCompletion()
    {
        var scheduler = new Scheduler();
        var log = new List<string>();
        scheduler.QueueStep(Leaf("simple", log));

        var state = scheduler.Pump();

        Assert.That(state, Is.EqualTo(StepState.Idle));
        Assert.That(log, Is.EqualTo(new[] { "simple" }));
    }

    [Test]
    public void Step_CanQueueASubStep_WhichRunsBeforeItResumes()
    {
        var scheduler = new Scheduler();
        var log = new List<string>();

        IEnumerator Parent()
        {
            log.Add("parent-before");
            scheduler.QueueStep(Leaf("child", log));
            yield return null;
            log.Add("parent-after");
        }

        scheduler.QueueStep(Parent());
        var state = scheduler.Pump();

        Assert.That(state, Is.EqualTo(StepState.Idle));
        Assert.That(log, Is.EqualTo(new[] { "parent-before", "child", "parent-after" }));
    }

    [Test]
    public void NestedInterrupts_ResumeAtTheCorrectPoint()
    {
        // Mirrors an interrupt opening inside a reaction opening inside an action:
        // each level queues the next level down, then must resume exactly where it
        // paused once the nested window closes.
        var scheduler = new Scheduler();
        var log = new List<string>();

        IEnumerator Action()
        {
            log.Add("action-before");
            scheduler.QueueStep(Reaction());
            yield return null;
            log.Add("action-after");
        }

        IEnumerator Reaction()
        {
            log.Add("reaction-before");
            scheduler.QueueStep(Leaf("interrupt", log));
            yield return null;
            log.Add("reaction-after");
        }

        scheduler.QueueStep(Action());
        var state = scheduler.Pump();

        Assert.That(state, Is.EqualTo(StepState.Idle));
        Assert.That(log, Is.EqualTo(new[]
        {
            "action-before",
            "reaction-before",
            "interrupt",
            "reaction-after",
            "action-after"
        }));
    }

    [Test]
    public void Step_BlocksOnAnUnresolvedPrompt_AndResumesOnlyAfterItCompletes()
    {
        var scheduler = new Scheduler();
        var log = new List<string>();
        var choice = new TaskCompletionSource<string>();

        IEnumerator WaitForPlayerChoice()
        {
            log.Add("prompted");
            yield return new StepAwait(choice.Task);
            log.Add($"resolved:{choice.Task.Result}");
        }

        scheduler.QueueStep(WaitForPlayerChoice());

        var blockedState = scheduler.Pump();
        Assert.That(blockedState, Is.EqualTo(StepState.Blocked));
        Assert.That(log, Is.EqualTo(new[] { "prompted" }), "must not resume before the prompt resolves");

        choice.SetResult("attack");
        var finalState = scheduler.Pump();

        Assert.That(finalState, Is.EqualTo(StepState.Idle));
        Assert.That(log, Is.EqualTo(new[] { "prompted", "resolved:attack" }));
    }

    private static IEnumerator Leaf(string message, List<string> log)
    {
        log.Add(message);
        yield break;
    }
}
