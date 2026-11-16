using System.Threading.Channels;

namespace Alis.Reactive.Conformance.App.World;

internal sealed class HostOrders
{
    private readonly Channel<HostOrder> _queued = Channel.CreateUnbounded<HostOrder>();
    private readonly Lock _work = new();
    private HostOrder? _inWork;

    internal async Task<string> Restart(string elementId, CancellationToken aborted)
    {
        var order = new HostOrder(elementId);
        if (!_queued.Writer.TryWrite(order)) throw new InvalidOperationException("The world ended before the host was told to restart '" + elementId + "'.");

        return await order.Report.Task.WaitAsync(aborted);
    }

    internal async Task<string?> Next(CancellationToken aborted)
    {
        while (await _queued.Reader.WaitToReadAsync(aborted))
        {
            lock (_work)
            {
                if (!_queued.Reader.TryRead(out var order)) continue;
                _inWork = order;
                return order.ElementId;
            }
        }

        return null;
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
        _queued.Writer.TryComplete();
        lock (_work)
        {
            while (_queued.Reader.TryRead(out var queued)) queued.Report.TrySetCanceled();
            _inWork?.Report.TrySetCanceled();
            _inWork = null;
        }
    }

    private sealed class HostOrder(string elementId)
    {
        internal string ElementId { get; } = elementId;

        internal TaskCompletionSource<string> Report { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
