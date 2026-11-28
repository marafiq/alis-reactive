namespace Alis.Reactive.Conformance.App.World;

internal sealed class CollectorChanges
{
    private TaskCompletionSource _next = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal Task Next => Volatile.Read(ref _next).Task;

    internal void Changed() => Interlocked.Exchange(ref _next, new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)).SetResult();
}
