using System.Diagnostics;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Tracing;

namespace Alis.Reactive.Conformance.App.World;

internal sealed class Collector(Worlds worlds, ExportedActivities exported)
{
    private const string Render = "alis.render";
    private const string Below = " > ";

    internal IReadOnlyList<CollectedSpan> SpansOf(string world)
        => [.. exported.Now().Where(activity => activity.Source.Name == ReactiveTracing.SourceName && IsOf(activity, world)).Select(CollectedSpan.Of),
            .. worlds.Of(world).Spans.Where(span => span.Scope == ReactiveTracing.SourceName)];

    internal string LinesOf(string world)
    {
        var spans = SpansOf(world);
        var listed = spans.Select(span => (span.TraceId, span.SpanId)).ToHashSet();
        var lines = new List<string>();
        foreach (var root in spans.Where(span => !listed.Contains((span.TraceId, span.ParentId)))) Walk(root, root.Name, spans, lines);
        return "<ol>" + string.Concat(lines.Select(line => "<li>" + WorldArea.Text(line) + "</li>")) + "</ol>";
    }

    internal string? Named(string world, string traceId, string spanId)
        => SpansOf(world).FirstOrDefault(span => span.TraceId == traceId && span.SpanId == spanId) is { } named ? LineOf(named.Name, named) : null;

    internal bool RenderedUnder(string world, string traceId) => SpansOf(world).Any(span => span.Name == Render && span.TraceId == traceId);

    private static void Walk(CollectedSpan span, string path, IReadOnlyList<CollectedSpan> spans, List<string> lines)
    {
        lines.Add(LineOf(path, span));
        foreach (var child in spans.Where(other => other.TraceId == span.TraceId && other.ParentId == span.SpanId).OrderBy(other => other.StartUnixNanos))
            Walk(child, path + Below + child.Name, spans, lines);
    }

    private static string LineOf(string names, CollectedSpan span) => span.Facts.Length == 0 ? names : names + " " + span.Facts;

    private static bool IsOf(Activity activity, string world) => IsTagged(activity, world) || (activity.Parent is { } parent && IsTagged(parent, world));

    private static bool IsTagged(Activity activity, string world) => activity.GetTagItem(WorldKey.Tag) is string tagged && tagged == world;
}
