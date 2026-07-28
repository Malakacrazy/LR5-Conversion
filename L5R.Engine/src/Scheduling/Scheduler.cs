using System.Collections;

namespace L5R.Engine.Scheduling;

/// <summary>
/// Ports ringteki's GamePipeline (server/game/gamepipeline.js): a nestable, pausable
/// queue of steps. Steps are plain C# iterator methods (<see cref="IEnumerator"/>) pumped
/// by hand via MoveNext() - no StartCoroutine/MonoBehaviour. A step nests a sub-step by
/// yielding another IEnumerator; it pauses for a player prompt by yielding a
/// <see cref="StepAwait"/> wrapping a TaskCompletionSource's Task.
///
/// QueueStep goes through a side buffer (mirrors ringteki's `this.queue`) instead of
/// pushing onto the frame stack directly. That matters when a step queues a sub-step
/// *while it's running* (e.g. an event fired synchronously mid-resolution triggers an
/// interrupt window that queues an AbilityResolver): the currently-executing frame is
/// captured by reference before MoveNext() runs, so it gets popped or kept based on its
/// own result regardless of what the reentrant QueueStep call did; newly queued frames
/// are only pushed on top afterwards, ahead of whatever remains.
/// </summary>
public sealed class Scheduler
{
    private readonly Stack<IEnumerator> _frames = new();
    private readonly Queue<IEnumerator> _pending = new();

    public bool IsIdle => _frames.Count == 0 && _pending.Count == 0;

    /// <summary>
    /// Queues a step to run next, ahead of whatever is currently paused. Safe to call
    /// from inside a running step's own body (including from a synchronous event handler
    /// invoked as a side effect of that step) — see class remarks.
    /// </summary>
    public void QueueStep(IEnumerator step) => _pending.Enqueue(step);

    /// <summary>Runs a single unit of work. See <see cref="StepState"/> for the result meaning.</summary>
    public StepState Advance()
    {
        if (_frames.Count == 0)
        {
            DrainPending();
            return _frames.Count > 0 ? StepState.Progressed : StepState.Idle;
        }

        var top = _frames.Peek();

        if (top.Current is StepAwait awaited && !awaited.IsReady)
            return StepState.Blocked;

        bool hasMore = top.MoveNext();

        if (!hasMore)
            _frames.Pop();

        DrainPending();

        return _frames.Count > 0 ? StepState.Progressed : StepState.Idle;
    }

    /// <summary>
    /// Runs until the pipeline is idle or genuinely blocked on external input. Used for
    /// headless simulation (bot-vs-bot, replay) where nothing else paces the pump.
    /// </summary>
    public StepState Pump()
    {
        StepState state;
        while ((state = Advance()) == StepState.Progressed) { }
        return state;
    }

    private void DrainPending()
    {
        if (_pending.Count == 0)
            return;

        var batch = new List<IEnumerator>(_pending.Count);
        while (_pending.Count > 0)
            batch.Add(_pending.Dequeue());

        // Preserve FIFO order among the batch: the first one queued ends up on top,
        // so it's the next one the scheduler resumes.
        for (int i = batch.Count - 1; i >= 0; i--)
            _frames.Push(batch[i]);
    }
}
