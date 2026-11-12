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
    private bool _rendered;

    internal PlanContent(string planId, Culture culture, TimeZone timeZone)
    {
        _planId = planId;
        _culture = culture;
        _timeZone = timeZone;
    }

    internal void Uses(string vendorName, Vendor vendor)
    {
        RefuseOnceRendered();
        _vendors[vendorName] = vendor;
    }

    internal void Declares(string objectId, Locator locator)
    {
        RefuseOnceRendered();
        _objects[objectId] = locator;
    }

    internal void Handles(Handler handler)
    {
        RefuseOnceRendered();
        _handlers.Add(handler);
    }

    internal PlanDocument Document(Trace trace)
    {
        if (_rendered)
        {
            throw new InvalidOperationException(
                $"The plan of '{_planId}' is rendered twice, so every handler it holds would run twice; render it once, as the view's last line.");
        }

        _rendered = true;
        return new(_planId, _culture, _timeZone, trace, _vendors, _objects, _handlers);
    }

    private void RefuseOnceRendered()
    {
        if (!_rendered) return;
        throw new InvalidOperationException(
            $"A door takes the plan of '{_planId}' after it rendered, so the page never runs what the door writes; " +
            "write every door above Html.RenderPlan, and where a @section holds one, render the plan in a section the layout renders after it.");
    }
}
