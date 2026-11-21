namespace Alis.Reactive.Conformance.App.World;

internal static class TraceEcho
{
    private const char Apart = '-';
    private const int Parts = 4;

    internal static string Traceparent(HttpRequest request, string world, Collector collector) => Traceparent(request.Headers.TraceParent, world, collector);

    internal static string Baggage(HttpRequest request) => Baggage(request.Headers.Baggage);

    internal static string Traceparent(string? header, string world, Collector collector)
    {
        if (header is null) return "traceparent none";
        var parts = header.Split(Apart);
        if (parts.Length != Parts) return "traceparent malformed";

        var (traceId, spanId, flags) = (parts[1], parts[2], parts[3]);
        var trace = collector.Named(world, traceId, spanId) is { } named ? "naming " + named
            : collector.RenderedUnder(world, traceId) ? "in the page's trace"
            : "in a trace of its own";
        return "traceparent flags " + flags + " " + trace;
    }

    internal static string Baggage(string? header) => header is null ? "baggage none" : "baggage " + header;
}
