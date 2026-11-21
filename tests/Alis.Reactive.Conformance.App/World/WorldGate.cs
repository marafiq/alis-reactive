namespace Alis.Reactive.Conformance.App.World;

internal sealed class WorldGate
{
    private readonly TaskCompletionSource _held = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _released = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal Task Held => _held.Task;

    internal bool IsParked => _held.Task.IsCompleted && !_released.Task.IsCompleted;

    internal Task Park()
    {
        _held.TrySetResult();
        return _released.Task;
    }

    internal void Release() => _released.TrySetResult();

    internal void EndWithItsWorld() => _released.TrySetCanceled();
}
