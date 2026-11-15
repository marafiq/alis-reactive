using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Alis.Reactive.PathsAndWire;
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
    private readonly Dictionary<string, string> _vendorDefinitions = new(StringComparer.Ordinal);
    private readonly HashSet<string> _renderedElements = new(StringComparer.Ordinal);
    private bool _rendered;

    internal PlanContent(string planId, CultureInfo rendersUnder, TimeZone timeZone, AppJson json)
    {
        _planId = planId;
        _culture = ViewCulture.Of(rendersUnder);
        RendersUnder = rendersUnder;
        _timeZone = timeZone;
        Json = json;
    }

    internal AppJson Json { get; }

    internal CultureInfo RendersUnder { get; }

    internal void Uses(string vendorName, (Vendor Vendor, IReadOnlyList<PathStep>? ElementPath, PluginLocator? Lookup) definition)
    {
        RefuseOnceRendered();
        var identity = JsonSerializer.Serialize(definition.Vendor, PlanJson.Options) + JsonSerializer.Serialize(definition.ElementPath, PlanJson.Options);
        if (_vendorDefinitions.TryGetValue(vendorName, out var first) && first != identity)
            throw new InvalidOperationException($"Two different vendors are named '{vendorName}'. Give each vendor its own name.");
        _vendorDefinitions[vendorName] = identity;
        _vendors[vendorName] = definition.Vendor;
        if (definition.Lookup is { } lookup) Declares("$plugin:" + lookup.Name, lookup);
    }

    internal void Declares(string objectId, Locator locator, bool rendered = false)
    {
        RefuseOnceRendered();
        if (_objects.TryGetValue(objectId, out var first) && first is ElementLocator previous && locator is ElementLocator next && previous.Vendor != next.Vendor)
            throw new InvalidOperationException($"Element '{objectId}' is named as a component of vendor '{previous.Vendor}' and as one of vendor '{next.Vendor}', but an element holds one component: name it by the declaration of the component its view renders.");
        if (rendered && !_renderedElements.Add(objectId))
            throw new InvalidOperationException($"Element '{objectId}' renders a second time in this plan: render each component once.");
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
