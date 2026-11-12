using System.Collections.Generic;

namespace Alis.Reactive.Schema;

internal sealed class PlanDocument
{
    internal const int CurrentVersion = 7;

    internal PlanDocument(
        string planId,
        Culture culture,
        TimeZone timeZone,
        Trace trace,
        IReadOnlyDictionary<string, Vendor> vendors,
        IReadOnlyDictionary<string, Locator> objects,
        IReadOnlyList<Handler> handlers)
    {
        PlanId = planId;
        Culture = culture;
        TimeZone = timeZone;
        Trace = trace;
        Vendors = vendors;
        Objects = objects;
        Handlers = handlers;
    }

    public int Version => CurrentVersion;

    public string PlanId { get; }

    public Culture Culture { get; }

    public TimeZone TimeZone { get; }

    public Trace Trace { get; }

    public IReadOnlyDictionary<string, Vendor> Vendors { get; }

    public IReadOnlyDictionary<string, Locator> Objects { get; }

    public IReadOnlyList<Handler> Handlers { get; }
}
