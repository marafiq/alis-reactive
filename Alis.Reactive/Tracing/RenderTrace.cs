using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Alis.Reactive.Schema;
using Trace = Alis.Reactive.Schema.Trace;

namespace Alis.Reactive.Tracing;

internal static class RenderTrace
{
    private const string Version = "00";
    private const string Sampled = "01";

    internal static Trace Of(Activity? request, Activity? render, IReadOnlyList<(string File, int Line)> calls, Func<string> view, Func<bool> console)
    {
        if (request is not { Recorded: true }) return OffTrace.Instance;
        if (request.IdFormat != ActivityIdFormat.W3C)
        {
            throw new InvalidOperationException(
                "The request's activity is recorded under a hierarchical id, which W3C Trace Context cannot carry to the page: keep Activity.DefaultIdFormat at its W3C default.");
        }

        var context = render is { Recorded: true } ? render : request;
        var viewPath = view();
        return new OnTrace(TraceparentOf(context), context.TraceStateString ?? "", console(), calls.Select((call, at) => SourceOf(call, at, viewPath)).ToList());
    }

    private static string TraceparentOf(Activity context)
        => Version + "-" + context.TraceId.ToHexString() + "-" + context.SpanId.ToHexString() + "-" + Sampled;

    private static Source SourceOf((string File, int Line) call, int at, string view)
    {
        var theCallIsTheViewsOwn = ("/" + call.File.Replace('\\', '/')).EndsWith(view, StringComparison.OrdinalIgnoreCase);
        return new Source(view, theCallIsTheViewsOwn ? call.Line : at + 1);
    }
}
