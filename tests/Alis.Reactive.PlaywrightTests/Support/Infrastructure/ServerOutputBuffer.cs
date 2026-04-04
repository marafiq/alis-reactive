using System.Text;

namespace Alis.Reactive.PlaywrightTests.Support.Infrastructure;

internal sealed class ServerOutputBuffer
{
    private readonly object _lock = new();
    private readonly Queue<string> _lines = new();
    private readonly int _maxLines;

    internal ServerOutputBuffer(int maxLines = 200)
    {
        _maxLines = maxLines;
    }

    internal void Clear()
    {
        lock (_lock)
        {
            _lines.Clear();
        }
    }

    internal void Record(string stream, string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return;

        lock (_lock)
        {
            _lines.Enqueue($"[{stream}] {line}");
            while (_lines.Count > _maxLines)
                _lines.Dequeue();
        }
    }

    internal Exception BuildStartupException(string message)
    {
        var snapshot = Snapshot();
        if (snapshot.Count == 0)
            return new Exception(message);

        var builder = new StringBuilder()
            .AppendLine(message)
            .AppendLine("Recent SandboxApp output:");

        foreach (var line in snapshot)
            builder.AppendLine(line);

        return new Exception(builder.ToString());
    }

    internal IReadOnlyCollection<string> Snapshot()
    {
        lock (_lock)
        {
            return _lines.ToArray();
        }
    }
}
