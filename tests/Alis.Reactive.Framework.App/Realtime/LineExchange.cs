using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Alis.Reactive.Framework.App.Realtime;

/// <summary>
/// The server's side of every page's line (a SignalR hub) and board (a server-sent event stream), each
/// page's kept apart by its world: the calls the server took, the connections it holds open, whether it
/// refuses to take the page's calls and how many it refused, and the pushes it sends.
/// </summary>
public sealed class LineExchange
{
    private readonly ConcurrentDictionary<string, LineWorld> _worlds = new();

    public LineWorld World(string world) => _worlds.GetOrAdd(world, _ => new LineWorld());
}

/// <summary>One page's line and board, as the server keeps them.</summary>
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

    /// <summary>Refuses the next calls to the line before the hub's handshake, as a server that is not ready yet.</summary>
    public void RefuseNegotiations(int count)
    {
        lock (_gate) _negotiationsToRefuse = count;
    }

    /// <summary>Whether the server refuses this call to the line: while hung up, or as one of the first calls it refuses.</summary>
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

    /// <summary>Counts a call to the line the server refused.</summary>
    public void LineCallRefused()
    {
        lock (_gate)
        {
            _refusedLineCalls++;
            Announce();
        }
    }

    /// <summary>The server hung up on the line: it takes no call from the page until it answers again.</summary>
    public void HangUp()
    {
        lock (_gate) _hungUp = true;
    }

    public void Answer()
    {
        lock (_gate) _hungUp = false;
    }

    /// <summary>Takes a call on the line and returns its number.</summary>
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

    /// <summary>The server closed the board for good: the page's next call is answered 204, which a browser does not retry.</summary>
    public bool BoardClosed
    {
        get { lock (_gate) return _boardClosed; }
    }

    /// <summary>Closes the board for good, and ends every open board response.</summary>
    public void CloseBoard()
    {
        lock (_gate) _boardClosed = true;
        EndBoard();
    }

    public void OpenBoard()
    {
        lock (_gate) _boardClosed = false;
    }

    /// <summary>A board response the page holds open: its number, its pushes, and its end.</summary>
    public BoardCall BoardOpened(CancellationToken stopped)
    {
        lock (_gate)
        {
            var call = new BoardCall(this, ++_boardCalls, stopped);
            _boards.Add(call);
            return call;
        }
    }

    /// <summary>Counts the board response as open once its first event is on its way.</summary>
    internal void BoardAnswered(BoardCall call)
    {
        lock (_gate)
        {
            call.Answered = true;
            Announce();
        }
    }

    // The response ends: it is no longer counted, and nothing ends it again.
    internal void BoardClosedBy(BoardCall call)
    {
        lock (_gate)
        {
            _boards.Remove(call);
            if (call.Answered) Announce();
        }
    }

    /// <summary>Ends every open board response, as a server that finishes a stream; a browser calls again.</summary>
    public void EndBoard()
    {
        lock (_gate)
        {
            foreach (var call in _boards) call.End();
        }
    }

    /// <summary>Pushes an event to every open board response, its data as written.</summary>
    public void PushToBoard(string name, string data)
    {
        lock (_gate)
        {
            foreach (var call in _boards) call.Pushes.Writer.TryWrite((name, data));
        }
    }

    /// <summary>
    /// Watches how many line and board connections the server holds open and how many calls to the line it
    /// refused, starting with the counts now.
    /// </summary>
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

/// <summary>How many line and board connections the server holds open for a page, and how many calls to the line it refused.</summary>
public sealed record LineCount(int Hub, int Board, int Refused);

/// <summary>One board response the server holds open.</summary>
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

    /// <summary>Cancelled when the page leaves or the server ends the response.</summary>
    public CancellationToken Ended => _ended.Token;

    public Channel<(string Name, string Data)> Pushes { get; } = Channel.CreateUnbounded<(string, string)>();

    internal bool Answered { get; set; }

    /// <summary>The response's first event is on its way: the server counts it as open.</summary>
    public void Answer() => _world.BoardAnswered(this);

    internal void End() => _ended.Cancel();

    public void Dispose()
    {
        _world.BoardClosedBy(this);
        _ended.Dispose();
    }
}
