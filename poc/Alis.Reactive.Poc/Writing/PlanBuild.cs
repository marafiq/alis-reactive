using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Types;

namespace Alis.Reactive.Writing;

internal sealed class PlanBuild
{
    internal const string PayloadName = "e";

    internal const string ArgumentsName = "arguments";

    private readonly Dictionary<string, Vendor> _vendors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Locator> _objects = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _vendorOfElement = new(StringComparer.Ordinal);
    private readonly List<Handler> _handlers = [];
    private readonly Stack<List<Statement>> _open = new();
    private readonly Dictionary<string, List<Statement>> _resultBlocks = new(StringComparer.Ordinal);
    private int _items;
    private int _results;
    private int _readingItems;
    private int _answering;
    private int _answers;
    private bool _handlerAwaited;

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

        if (_answering > 0)
        {
            throw new InvalidOperationException(
                "A step is written inside a callback's answer, which answers its vendor with one value. Write the step in a pipeline.");
        }

        if (_open.Count == 0)
        {
            throw new InvalidOperationException(
                "A step is written inside a pipeline, the callback a trigger runs: .Reactive(c => c.Click, p => ...).");
        }

        _open.Peek().Add(statement);
        var awaits = statement is DoStatement { Await: true } or LetStatement { Await: true };
        _handlerAwaited = _handlerAwaited || awaits;
    }

    internal bool HandlerAwaited => _handlerAwaited;

    internal IReadOnlyList<Statement> HandlerBlock(Action body)
    {
        _handlerAwaited = false;
        return Block(body);
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

    internal string AnswerArgumentsName() => ArgumentsName + (++_answers).ToString(CultureInfo.InvariantCulture);

    internal Expression Result(CallExpression call)
    {
        if (_readingItems > 0 || _answering > 0) return call;
        return Let(call, awaits: false);
    }

    internal Expression AwaitedResult(CallExpression call)
    {
        if (_readingItems > 0)
        {
            throw new InvalidOperationException(
                "An async method is called inside a list's lambda, which reads every item in one turn and waits for nothing. Call the method in the pipeline and read its result in the lambda.");
        }

        if (_answering > 0)
        {
            throw new InvalidOperationException(
                "A callback answers its vendor at once, so its answer cannot wait for an async method or a request.");
        }

        return Let(call, awaits: true);
    }

    private RefExpression Let(CallExpression call, bool awaits)
    {
        var name = "result" + (++_results).ToString(CultureInfo.InvariantCulture);
        Write(new LetStatement(name, call, awaits));
        _resultBlocks[name] = _open.Peek();
        return new RefExpression(name);
    }

    internal Expression InScope(Expression read)
    {
        var readsAResult = read is RefExpression reference && _resultBlocks.TryGetValue(reference.Name, out var block) && !_open.Contains(block);
        if (!readsAResult) return read;
        throw new InvalidOperationException(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it.");
    }

    internal TRead Answering<TRead>(Func<TRead> answer)
    {
        _answering++;
        try
        {
            return answer();
        }
        finally
        {
            _answering--;
        }
    }

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

    internal void Add(Handler handler, Listening listening)
    {
        var heldByOneProperty = listening is AssignmentListening && _handlers.Exists(held => held.On == handler.On && held.Event == handler.Event);
        if (heldByOneProperty)
        {
            throw new InvalidOperationException(
                $"'{handler.Event}' is a callback property, which holds one listener, so a second handler would replace the first. Write both pipelines in one handler.");
        }

        _handlers.Add(handler);
    }

    internal PlanDocument Document() => new(PlanId, new PlanCulture(CollationOf(Culture)), _vendors, _objects, _handlers);

    private static string CollationOf(CultureInfo culture)
    {
        var isTheInvariantCulture = culture.Name.Length == 0;
        return isTheInvariantCulture ? "en" : culture.Name;
    }
}
