namespace L5R.Engine.Scheduling;

/// <summary>
/// Yielded by a step to pause the <see cref="Scheduler"/> until a pending player prompt
/// resolves, without spin-calling MoveNext(). Bridges the synchronous step body to
/// async I/O via a <see cref="TaskCompletionSource"/>-backed <see cref="Task"/>.
/// </summary>
public sealed class StepAwait
{
    public Task Task { get; }

    public StepAwait(Task task) => Task = task;

    public bool IsReady => Task.IsCompleted;
}
