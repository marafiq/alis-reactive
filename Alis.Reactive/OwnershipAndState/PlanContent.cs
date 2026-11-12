using System;
using System.Collections.Generic;
using Alis.Reactive.Schema;
using TimeZone = Alis.Reactive.Schema.TimeZone;

namespace Alis.Reactive.OwnershipAndState;

internal sealed class PlanContent
{
    private readonly string _planId;
    private readonly Culture _culture;
    private readonly TimeZone _timeZone;
    private readonly SortedDictionary<string, Vendor> _vendors = new(StringComparer.Ordinal);
    private readonly SortedDictionary<string, Locator> _objects = new(StringComparer.Ordinal);
    private readonly List<Handler> _handlers = [];

    internal PlanContent(string planId, Culture culture, TimeZone timeZone)
    {
        _planId = planId;
        _culture = culture;
        _timeZone = timeZone;
    }

    internal void Uses(string vendorName, Vendor vendor) => _vendors[vendorName] = vendor;

    internal void Declares(string objectId, Locator locator) => _objects[objectId] = locator;

    internal void Handles(Handler handler) => _handlers.Add(handler);

    internal PlanDocument Document(Trace trace) => new(_planId, _culture, _timeZone, trace, _vendors, _objects, _handlers);
}
