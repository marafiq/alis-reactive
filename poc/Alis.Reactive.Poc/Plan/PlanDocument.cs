using System.Collections.Generic;

namespace Alis.Reactive.Plan;

internal sealed class PlanDocument
{
    internal PlanDocument(string planId, PlanCulture culture, IReadOnlyDictionary<string, Vendor> vendors, IReadOnlyDictionary<string, Locator> objects, IReadOnlyList<Handler> handlers)
    {
        PlanId = planId;
        Culture = culture;
        Vendors = vendors;
        Objects = objects;
        Handlers = handlers;
    }

    public int Version => 9;

    public string PlanId { get; }

    public PlanCulture Culture { get; }

    public IReadOnlyDictionary<string, Vendor> Vendors { get; }

    public IReadOnlyDictionary<string, Locator> Objects { get; }

    public IReadOnlyList<Handler> Handlers { get; }
}
