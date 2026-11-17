using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Alis.Reactive.Conformance.App.World;

internal sealed class WorldStreams
{
    private readonly ConcurrentDictionary<string, WorldStream> _streams = new(StringComparer.Ordinal);

    internal WorldStream Of(string stream) => _streams.GetOrAdd(stream, _ => new WorldStream());

    internal void End()
    {
        foreach (var stream in _streams.Values) stream.End();
    }
}

internal sealed record Pushed(string Event, string Data);

internal sealed class WorldStream
{
    private readonly Lock _gate = new();
    private readonly List<Channel<Pushed>> _open = [];
    private readonly ConcurrentQueue<string> _traceparents = new();
    private Admission _admission = Admission.Open;
    private TaskCompletionSource _reopened = Signal();
    private TaskCompletionSource _changed = Signal();
    private int _accepted;
    private int _refused;
    private int _answered;
    private bool _nextHeld;

    internal int Accepted
    {
        get
        {
            lock (_gate) return _accepted;
        }
    }

    internal string FirstTraceparent => _traceparents.TryPeek(out var first) ? first : "none";

    internal void Records(string traceparent)
    {
        if (traceparent.Length > 0) _traceparents.Enqueue(traceparent);
    }

    internal async Task<int?> Admits(CancellationToken stopped)
    {
        Task reopened;
        lock (_gate)
        {
            switch (_admission)
            {
                case Admission.Refused:
                    _refused += 1;
                    _answered += 1;
                    Changed();
                    return null;
                case Admission.Open:
                    _accepted += 1;
                    _answered += 1;
                    Changed();
                    return _accepted;
            }

            _nextHeld = true;
            Changed();
            reopened = _reopened.Task;
        }

        await reopened.WaitAsync(stopped);
        lock (_gate)
        {
            _nextHeld = false;
            _accepted += 1;
            _answered += 1;
            Changed();
            return _accepted;
        }
    }

    internal ChannelReader<Pushed> Opens(CancellationToken stopped)
    {
        var channel = Channel.CreateUnbounded<Pushed>();
        lock (_gate)
        {
            _open.Add(channel);
            Changed();
        }

        stopped.Register(() => Ends(channel));
        return channel.Reader;
    }

    internal int Push(string @event, string data)
    {
        lock (_gate)
        {
            foreach (var connection in _open) connection.Writer.TryWrite(new Pushed(@event, data));
            return _open.Count;
        }
    }

    internal void Drop()
    {
        lock (_gate)
        {
            _admission = Admission.Held;
            _reopened = Signal();
            EndsEvery();
        }
    }

    internal void Refuse()
    {
        lock (_gate)
        {
            _admission = Admission.Refused;
            EndsEvery();
        }
    }

    internal void Reopen()
    {
        lock (_gate)
        {
            _admission = Admission.Open;
            _reopened.TrySetResult();
        }
    }

    internal async IAsyncEnumerable<int> OpenCounts([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken stopped)
    {
        var last = -1;
        while (true)
        {
            Task changed;
            int open;
            lock (_gate)
            {
                open = _open.Count;
                changed = _changed.Task;
            }

            if (open != last) yield return open;
            last = open;
            await changed.WaitAsync(stopped);
        }
    }

    internal Task Holds(int connections, CancellationToken stopped) => Until(() => _open.Count == connections, stopped);

    internal Task HoldsTheNext(CancellationToken stopped) => Until(() => _nextHeld, stopped);

    internal Task HasRefused(int connections, CancellationToken stopped) => Until(() => _refused >= connections, stopped);

    internal Task HasAnswered(int connections, CancellationToken stopped) => Until(() => _answered >= connections, stopped);

    internal void End()
    {
        lock (_gate)
        {
            _reopened.TrySetCanceled();
            EndsEvery();
        }
    }

    private async Task Until(Func<bool> holds, CancellationToken stopped)
    {
        while (true)
        {
            Task changed;
            lock (_gate)
            {
                if (holds()) return;
                changed = _changed.Task;
            }

            await changed.WaitAsync(stopped);
        }
    }

    private void Ends(Channel<Pushed> connection)
    {
        lock (_gate)
        {
            if (!_open.Remove(connection)) return;
            connection.Writer.TryComplete();
            Changed();
        }
    }

    private void EndsEvery()
    {
        foreach (var connection in _open) connection.Writer.TryComplete();
        _open.Clear();
        Changed();
    }

    private void Changed()
    {
        var changed = _changed;
        _changed = Signal();
        changed.TrySetResult();
    }

    private static TaskCompletionSource Signal() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private enum Admission
    {
        Open,
        Held,
        Refused,
    }
}
