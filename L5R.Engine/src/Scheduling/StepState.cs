namespace L5R.Engine.Scheduling;

public enum StepState
{
    /// <summary>No frames left to run.</summary>
    Idle,

    /// <summary>
    /// The running step is sitting on an unresolved <see cref="StepAwait"/>; do not call
    /// Advance() again until it resolves.
    /// </summary>
    Blocked,

    /// <summary>One unit of work ran. Call Advance() again to continue.</summary>
    Progressed
}
