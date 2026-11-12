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
    private readonly Dictionary<string, Vendor> _vendors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Locator> _objects = new(StringComparer.Ordinal);
    private readonly List<Handler> _handlers = [];

    internal PlanContent(string planId, Culture culture, TimeZone timeZone)
    {
        _planId = planId;
        _culture = culture;
        _timeZone = timeZone;
    }

    internal PlanDocument Document(Trace trace) => new(_planId, _culture, _timeZone, trace, _vendors, _objects, _handlers);
}
