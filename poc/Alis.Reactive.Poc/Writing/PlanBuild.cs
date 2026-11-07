using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Types;

namespace Alis.Reactive.Writing;

internal sealed class PlanBuild
{
    private readonly Dictionary<string, Vendor> _vendors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Locator> _objects = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _vendorOfElement = new(StringComparer.Ordinal);
    private readonly List<Handler> _handlers = [];
    private readonly Stack<List<Statement>> _open = new();
    private int _items;
    private int _readingItems;

    internal PlanBuild(string planId, JsonSerializerOptions values)
    {
        PlanId = planId;
        values.MakeReadOnly(populateMissingResolver: true);
        Values = values;
        Starts = new DataStarts(values);
        Culture = CultureInfo.CurrentCulture;
    }

    internal string PlanId { get; }

    internal JsonSerializerOptions Values { get; }

    internal DataStarts Starts { get; }

    internal CultureInfo Culture { get; }

    internal void Write(Statement statement)
    {
        if (_readingItems > 0)
        {
            throw new InvalidOperationException(
                "A step is written inside a list's lambda, which reads each item and writes nothing. Write the step in the pipeline.");
        }

        if (_open.Count == 0)
        {
            throw new InvalidOperationException(
                "A step is written inside a pipeline, the callback a trigger runs: .Reactive(c => c.Click, p => ...).");
        }

        _open.Peek().Add(statement);
    }

    internal IReadOnlyList<Statement> Block(Action body)
    {
        var block = new List<Statement>();
        _open.Push(block);
        try
        {
            body();
        }
        finally
        {
            _open.Pop();
        }

        return block;
    }

    internal string ItemName() => "item" + (++_items).ToString(CultureInfo.InvariantCulture);

    internal TRead ReadingItems<TRead>(Func<TRead> read)
    {
        _readingItems++;
        try
        {
            return read();
        }
        finally
        {
            _readingItems--;
        }
    }

    internal string Element(string elementId, string vendorName, Vendor vendor)
    {
        if (_vendors.TryGetValue(vendorName, out var known) && !ReferenceEquals(known, vendor))
        {
            throw new InvalidOperationException($"Two vendors are named '{vendorName}' on one page. A vendor's name is unique among the page's vendors.");
        }

        if (_vendorOfElement.TryGetValue(elementId, out var holder) && holder != vendorName)
        {
            throw new InvalidOperationException(
                $"The element '{elementId}' is declared with the vendor '{holder}' and with '{vendorName}'. An element holds one component.");
        }

        _vendors[vendorName] = vendor;
        _vendorOfElement[elementId] = vendorName;
        _objects[elementId] = new ElementLocator(elementId, vendorName);
        return elementId;
    }

    internal void Add(Handler handler) => _handlers.Add(handler);

    internal PlanDocument Document() => new(PlanId, new PlanCulture(CollationOf(Culture)), _vendors, _objects, _handlers);

    private static string CollationOf(CultureInfo culture)
    {
        var isTheInvariantCulture = culture.Name.Length == 0;
        return isTheInvariantCulture ? "en" : culture.Name;
    }
}
