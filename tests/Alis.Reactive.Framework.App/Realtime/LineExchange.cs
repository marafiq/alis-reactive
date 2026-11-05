using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Alis.Reactive.Framework.App.Realtime;

public sealed class LineExchange
{
    private readonly ConcurrentDictionary<string, LineWorld> _worlds = new();

    public LineWorld World(string world) => _worlds.GetOrAdd(world, _ => new LineWorld());
}

public sealed class LineWorld
{
    private readonly object _gate = new();
    private readonly HashSet<string> _lines = new();
    private readonly List<BoardCall> _boards = new();
    private readonly List<Channel<LineCount>> _watchers = new();
    private int _lineCalls;
    private int _refusedLineCalls;
    private int _boardCalls;
    private int _negotiationsToRefuse;
    private bool _hungUp;
    private bool _boardClosed;

    public void RefuseNegotiations(int count)
    {
        lock (_gate) _negotiationsToRefuse = count;
    }

    public bool RefusesNegotiation()
    {
        lock (_gate)
        {
            if (_hungUp) return true;
            if (_negotiationsToRefuse == 0) return false;
            _negotiationsToRefuse--;
            return true;
        }
    }

    public void LineCallRefused()
    {
        lock (_gate)
        {
            _refusedLineCalls++;
            Announce();
        }
    }

    public void HangUp()
    {
        lock (_gate) _hungUp = true;
    }

    public void Answer()
    {
        lock (_gate) _hungUp = false;
    }

    public int LineOpened(string connectionId)
    {
        lock (_gate)
        {
            _lines.Add(connectionId);
            Announce();
            return ++_lineCalls;
        }
    }

    public void LineClosed(string connectionId)
    {
        lock (_gate)
        {
            if (_lines.Remove(connectionId)) Announce();
        }
    }

    public bool BoardClosed
    {
        get { lock (_gate) return _boardClosed; }
    }

    public void CloseBoard()
    {
        lock (_gate) _boardClosed = true;
        EndBoard();
    }

    public void OpenBoard()
    {
        lock (_gate) _boardClosed = false;
    }

    public BoardCall BoardOpened(CancellationToken stopped)
    {
        lock (_gate)
        {
            var call = new BoardCall(this, ++_boardCalls, stopped);
            _boards.Add(call);
            return call;
        }
    }

    internal void BoardAnswered(BoardCall call)
    {
        lock (_gate)
        {
            call.Answered = true;
            Announce();
        }
    }

    internal void BoardClosedBy(BoardCall call)
    {
        lock (_gate)
        {
            _boards.Remove(call);
            if (call.Answered) Announce();
        }
    }

    public void EndBoard()
    {
        lock (_gate)
        {
            foreach (var call in _boards) call.End();
        }
    }

    public void PushToBoard(string name, string data)
    {
        lock (_gate)
        {
            foreach (var call in _boards) call.Pushes.Writer.TryWrite((name, data));
        }
    }

    public Channel<LineCount> Watch()
    {
        var watcher = Channel.CreateUnbounded<LineCount>();
        lock (_gate)
        {
            _watchers.Add(watcher);
            watcher.Writer.TryWrite(Count());
        }
        return watcher;
    }

    public void Unwatch(Channel<LineCount> watcher)
    {
        lock (_gate) _watchers.Remove(watcher);
    }

    private void Announce()
    {
        var count = Count();
        foreach (var watcher in _watchers) watcher.Writer.TryWrite(count);
    }

    private LineCount Count() => new(_lines.Count, _boards.Count(call => call.Answered), _refusedLineCalls);
}

public sealed record LineCount(int Hub, int Board, int Refused);

public sealed class BoardCall : IDisposable
{
    private readonly LineWorld _world;
    private readonly CancellationTokenSource _ended;

    internal BoardCall(LineWorld world, int number, CancellationToken stopped)
    {
        _world = world;
        Number = number;
        _ended = CancellationTokenSource.CreateLinkedTokenSource(stopped);
    }

    public int Number { get; }

    public CancellationToken Ended => _ended.Token;

    public Channel<(string Name, string Data)> Pushes { get; } = Channel.CreateUnbounded<(string, string)>();

    internal bool Answered { get; set; }

    public void Answer() => _world.BoardAnswered(this);

    internal void End() => _ended.Cancel();

    public void Dispose()
    {
        _world.BoardClosedBy(this);
        _ended.Dispose();
    }
}
