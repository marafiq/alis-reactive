using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using TimeZone = Alis.Reactive.Schema.TimeZone;

namespace Alis.Reactive.OwnershipAndState;

internal sealed class PlanContent
{
    private const string PluginObjectId = "$plugin:";

    private readonly string _planId;
    private readonly Func<Type, bool> _namesFlags;
    private readonly Culture _culture;
    private readonly TimeZone _timeZone;
    private readonly SortedDictionary<string, Vendor> _vendors = new(StringComparer.Ordinal);
    private readonly SortedDictionary<string, Locator> _objects = new(StringComparer.Ordinal);
    private readonly List<Func<Handler>> _handlers = [];
    private readonly List<(string File, int Line)> _calls = [];
    private readonly Dictionary<string, string> _vendorDefinitions = new(StringComparer.Ordinal);
    private readonly HashSet<string> _renderedElements = new(StringComparer.Ordinal);
    private readonly Dictionary<string, (string Listening, string Event, string HeardBy)> _heardPlugins = new(StringComparer.Ordinal);
    private readonly SortedDictionary<string, InputBinding> _inputs = new(StringComparer.Ordinal);
    private readonly SortedDictionary<string, Container> _containers = new(StringComparer.Ordinal);
    private readonly HashSet<string> _slots = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _contentSets = new(StringComparer.Ordinal);
    private readonly Func<string, (string Prefix, string Name, string Id)> _nameInput;
    private readonly Func<IServiceProvider> _services;
    private readonly Func<string> _pathBase;
    private readonly Lazy<PageAntiforgery> _antiforgery;
    private bool _rendered;

    internal PlanContent(string planId, CultureInfo rendersUnder, TimeZone timeZone, AppJson json, Func<string, (string Prefix, string Name, string Id)> nameInput, Func<Type, bool> namesFlags,
        Func<IServiceProvider> services, Func<PageAntiforgery> antiforgery, Func<string> pathBase)
    {
        _planId = planId;
        _culture = ViewCulture.Of(rendersUnder);
        RendersUnder = rendersUnder;
        _timeZone = timeZone;
        Json = json;
        _nameInput = nameInput;
        _namesFlags = namesFlags;
        _services = services;
        _pathBase = pathBase;
        _antiforgery = new Lazy<PageAntiforgery>(antiforgery);
    }

    internal IServiceProvider Services => _services();

    internal string PathBase => _pathBase();

    internal AppJson Json { get; }

    internal PageAntiforgery Antiforgery => _antiforgery.Value;

    internal bool NamesFlags(Type type) => _namesFlags(type);

    internal CultureInfo RendersUnder { get; }

    internal string Model => _planId;

    internal (string Prefix, string Name, string Id) NameInput(string text) => _nameInput(text);

    internal ModelPluginData ModelData => new(_inputs, _containers);

    internal void RendersContainer(string id, Container container)
    {
        RefuseOnceRendered();
        RendersInputElement(id, id);
        RendersInputElement(container.Summary, id);
        _containers.Add(id, container);
    }

    internal void RendersInputElement(string id, string name)
    {
        RefuseOnceRendered();
        if (_renderedElements.Add(id)) return;
        throw new InvalidOperationException(
            $"Input '{name}' renders element id '{id}' a second time in this plan: render the field once, or each copy for its own member with Html.PartialFor.");
    }

    internal void Uses(string vendorName, (Vendor Vendor, IReadOnlyList<PathStep>? ElementPath, PluginLocator? Lookup) definition)
    {
        RefuseOnceRendered();
        var identity = JsonSerializer.Serialize(definition.Vendor, PlanJson.Options) + JsonSerializer.Serialize(definition.ElementPath, PlanJson.Options);
        if (_vendorDefinitions.TryGetValue(vendorName, out var first) && first != identity)
            throw new InvalidOperationException($"Two different vendors are named '{vendorName}'. Give each vendor its own name.");
        _vendorDefinitions[vendorName] = identity;
        _vendors[vendorName] = definition.Vendor;
        if (definition.Lookup is { } lookup) ReachesPlugin(lookup);
    }

    internal string ReachesPlugin(PluginLocator reached)
    {
        RefuseOnceRendered();
        var objectId = PluginObjectId + reached.Name;
        if (!_objects.ContainsKey(objectId)) _objects[objectId] = reached;
        return objectId;
    }

    internal void HearsPlugin(PluginLocator heard, string raised, string heardBy)
    {
        RefuseOnceRendered();
        var objectId = PluginObjectId + heard.Name;
        var listening = JsonSerializer.Serialize(heard.Listen, PlanJson.Options);
        var heardBefore = _heardPlugins.TryGetValue(objectId, out var first);
        if (heardBefore && first.Listening != listening)
        {
            throw new InvalidOperationException(
                $"Two declarations of the plugin '{heard.Name}' are heard two ways in one plan: the one whose event is '{first.Event}' by {first.HeardBy}, " +
                $"and the one whose event is '{raised}' by {heardBy}. A plugin is one object: give every declaration of '{heard.Name}' that declares an event one EventListening.");
        }

        if (heardBefore) return;
        _heardPlugins[objectId] = (listening, raised, heardBy);
        _objects[objectId] = heard;
    }

    internal void Declares(string objectId, Locator locator, bool rendered = false)
    {
        RefuseOnceRendered();
        if (_objects.TryGetValue(objectId, out var first) && first is ElementLocator previous && locator is ElementLocator next && previous.Vendor != next.Vendor)
            throw new InvalidOperationException($"Element '{objectId}' is named as a component of vendor '{previous.Vendor}' and as one of vendor '{next.Vendor}', but an element holds one component: name it by the declaration of the component its view renders.");
        if (rendered) RendersElement(objectId);
        _objects[objectId] = locator;
    }

    internal void RendersElement(string elementId)
    {
        RefuseOnceRendered();
        if (!_renderedElements.Add(elementId))
            throw new InvalidOperationException($"Element '{elementId}' renders a second time in this plan: render each component once.");
    }

    internal bool HasRendered(string elementId) => _renderedElements.Contains(elementId);

    internal void LoadsOrUnloads(string elementId)
    {
        RefuseOnceRendered();
        if (_contentSets.TryGetValue(elementId, out var content)) throw ContentOfASlot(elementId, content);
        _slots.Add(elementId);
    }

    internal void SetsContentOf(string elementId, string content)
    {
        RefuseOnceRendered();
        if (_slots.Contains(elementId)) throw ContentOfASlot(elementId, content);
        if (!_contentSets.ContainsKey(elementId)) _contentSets.Add(elementId, content);
    }

    internal void BindsInput(string inputId, InputBinding binding)
    {
        RefuseOnceRendered();
        if (binding.Message.Length > 0) RendersInputElement(binding.Message, binding.Name);
        _inputs[inputId] = binding;
    }

    internal void Handles(Handler handler, string callerFile, int callerLine) => HandlesWhenRendered(() => handler, callerFile, callerLine);

    internal void HandlesWhenRendered(Func<Handler> handler, string callerFile, int callerLine)
    {
        RefuseOnceRendered();
        _handlers.Add(handler);
        _calls.Add((callerFile, callerLine));
    }

    internal PlanDocument Document(Func<IReadOnlyList<(string File, int Line)>, Trace> traceOf)
    {
        if (_rendered)
        {
            throw new InvalidOperationException(
                $"The plan of '{_planId}' is rendered twice, so every handler it holds would run twice; render it once, as the view's last line.");
        }

        _rendered = true;
        return new(_planId, _culture, _timeZone, traceOf(_calls), _vendors, _objects, _handlers.Select(handler => handler()).ToArray());
    }

    private static InvalidOperationException ContentOfASlot(string elementId, string content)
        => new($"The plan sets the {content} of element '{elementId}', a slot this plan loads or unloads, so what the slot loaded would keep running under content it no longer shows. " +
            $"Empty the slot with Unload(\"{elementId}\"), and write the {content} into an element beside it.");

    private void RefuseOnceRendered()
    {
        if (!_rendered) return;
        throw new InvalidOperationException(
            $"A door takes the plan of '{_planId}' after it rendered, so the page never runs what the door writes; " +
            "write every door above Html.RenderPlan, and where a @section holds one, render the plan in a section the layout renders after it.");
    }
}
