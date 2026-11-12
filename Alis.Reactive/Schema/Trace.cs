using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Schema;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(OffTrace), "off")]
[JsonDerivedType(typeof(OnTrace), "on")]
internal abstract class Trace
{
    private protected Trace()
    {
    }
}

internal sealed class OffTrace : Trace
{
    internal static readonly OffTrace Instance = new();

    private OffTrace()
    {
    }
}

internal sealed class OnTrace : Trace
{
    internal OnTrace(string traceparent, string tracestate, bool console, IReadOnlyList<Source> sources)
    {
        Traceparent = traceparent;
        Tracestate = tracestate;
        Console = console;
        Sources = sources;
    }

    public string Traceparent { get; }

    public string Tracestate { get; }

    public bool Console { get; }

    public IReadOnlyList<Source> Sources { get; }
}

internal sealed class Source
{
    internal Source(string view, int line)
    {
        View = view;
        Line = line;
    }

    public string View { get; }

    public int Line { get; }
}
