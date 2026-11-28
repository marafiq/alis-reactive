namespace Alis.Reactive.Conformance.App.World;

internal sealed class TwinHostOrders
{
    private readonly object _work = new();
    private readonly Queue<HostOrder> _queued = new();
    private readonly SemaphoreSlim _ordered = new(0);
    private HostOrder? _inWork;
    private bool _ended;

    internal async Task<string> Restart(string elementId, CancellationToken aborted)
    {
        var order = new HostOrder(elementId);
        lock (_work)
        {
            if (_ended) throw new InvalidOperationException("The world ended before the host was told to restart '" + elementId + "'.");
            _queued.Enqueue(order);
        }

        _ordered.Release();
        using (aborted.Register(() => order.Report.TrySetCanceled()))
            return await order.Report.Task;
    }

    internal async Task<string?> Next(CancellationToken aborted)
    {
        await _ordered.WaitAsync(aborted);
        lock (_work)
        {
            if (_queued.Count == 0) return null;
            _inWork = _queued.Dequeue();
            return _inWork.ElementId;
        }
    }

    internal bool Done(string report)
    {
        lock (_work)
        {
            if (_inWork is not { } order) return false;
            _inWork = null;
            order.Report.TrySetResult(report);
            return true;
        }
    }

    internal void End()
    {
        lock (_work)
        {
            _ended = true;
            while (_queued.Count > 0) _queued.Dequeue().Report.TrySetCanceled();
            _inWork?.Report.TrySetCanceled();
            _inWork = null;
        }

        _ordered.Release(int.MaxValue - _ordered.CurrentCount);
    }

    private sealed class HostOrder(string elementId)
    {
        internal string ElementId { get; } = elementId;

        internal TaskCompletionSource<string> Report { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
