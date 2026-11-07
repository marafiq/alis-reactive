using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Alis.Reactive.Inputs;
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
    private readonly HashSet<string> _rendered = new(StringComparer.Ordinal);
    private readonly List<Handler> _handlers = [];
    private readonly Stack<List<Statement>> _open = new();
    private readonly Dictionary<string, List<Statement>> _resultBlocks = new(StringComparer.Ordinal);
    private readonly ResultLines _resultLines = new();
    private readonly HashSet<string> _elseIfResults = new(StringComparer.Ordinal);
    private int _items;
    private int _results;
    private int _values;
    private int _readingItems;
    private int _answering;
    private int _answers;
    private bool _handlerAwaited;

    internal PlanBuild(string planId, JsonSerializerOptions values, InputNames names)
    {
        PlanId = planId;
        Names = names;
        values.MakeReadOnly(populateMissingResolver: true);
        Values = values;
        Starts = new DataStarts(values);
        Culture = CultureInfo.CurrentCulture;
    }

    internal string PlanId { get; }

    internal InputNames Names { get; }

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

        var block = _open.Peek();
        _resultLines.Written(statement, block);
        block.Add(statement);
        var awaits = statement is DoStatement { Await: true } or LetStatement { Await: true };
        _handlerAwaited = _handlerAwaited || awaits;
    }

    internal bool HandlerAwaited => _handlerAwaited;

    internal IReadOnlyList<Statement> HandlerBlock(Action body)
    {
        _handlerAwaited = false;
        return Block(body);
    }

    internal List<Statement> Block(Action body)
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

    internal RefExpression Bind(Expression value)
    {
        var name = "value" + (++_values).ToString(CultureInfo.InvariantCulture);
        Write(new LetStatement(name, value, awaits: false));
        return new RefExpression(name);
    }

    internal Expression InScope(Expression read)
    {
        if (read is not RefExpression reference) return read;
        var readsAResultOutsideItsBlock = _resultBlocks.TryGetValue(reference.Name, out var block) && !_open.Contains(block);
        if (!readsAResultOutsideItsBlock) return read;
        if (_elseIfResults.Contains(reference.Name))
        {
            throw new InvalidOperationException(
                "A method's result called in an ElseIf's condition is read after its When, and the call runs only when the ElseIf is reached: call the method before the When to read its result after it.");
        }

        throw new InvalidOperationException(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it.");
    }

    internal Branching When(Expression condition, Action then)
    {
        var awaitedBefore = _handlerAwaited;
        var (statement, armAwaited) = If(condition, then);
        var branching = new Branching(statement, _open.Peek(), awaitedBefore, armAwaited);
        _handlerAwaited = branching.AwaitedAfter;
        return branching;
    }

    internal void ElseIf(Branching branching, Expression condition, Action then)
    {
        var lets = ConditionLets(WrittenAfter(branching), condition);
        var alternative = new List<Statement>();
        foreach (var let in lets)
        {
            branching.Block.Remove(let);
            alternative.Add(let);
            _resultBlocks[let.Name] = alternative;
            _elseIfResults.Add(let.Name);
        }

        var entered = branching.ElseEntered || lets.Exists(let => let.Await);
        _handlerAwaited = entered;
        _open.Push(alternative);
        try
        {
            var (statement, armAwaited) = If(condition, then);
            branching.ElseIf(alternative, statement, entered, armAwaited);
        }
        finally
        {
            _open.Pop();
        }

        _handlerAwaited = branching.AwaitedAfter;
    }

    internal void Else(Branching branching, Action otherwise)
    {
        if (WrittenAfter(branching).Count > 0) throw new InvalidOperationException(ContinuesItsWhen);
        _handlerAwaited = branching.ElseEntered;
        var block = Block(otherwise);
        branching.Else(block, _handlerAwaited);
        _handlerAwaited = branching.AwaitedAfter;
    }

    private const string ContinuesItsWhen =
        "An ElseIf or an Else continues the When it follows, before any other step of its block: write it in the same sentence.";

    private (IfStatement Statement, bool ArmAwaited) If(Expression condition, Action then)
    {
        var entered = _handlerAwaited;
        var block = Block(then);
        var armAwaited = _handlerAwaited;
        var statement = new IfStatement(condition, block);
        _handlerAwaited = entered;
        Write(statement);
        return (statement, armAwaited);
    }

    private List<Statement> WrittenAfter(Branching branching)
    {
        if (branching.Ended)
        {
            throw new InvalidOperationException(
                "An Else ends its When: write each ElseIf before it, and every step of the remaining case in that one Else.");
        }

        var continuesItsBlock = _open.Count > 0 && ReferenceEquals(_open.Peek(), branching.Block);
        if (!continuesItsBlock) throw new InvalidOperationException(ContinuesItsWhen);
        var after = branching.Block.IndexOf(branching.Outer) + 1;
        return branching.Block.GetRange(after, branching.Block.Count - after);
    }

    private static List<LetStatement> ConditionLets(List<Statement> written, Expression condition)
    {
        var lets = new List<LetStatement>();
        foreach (var statement in written)
        {
            if (statement is not LetStatement let) throw new InvalidOperationException(ContinuesItsWhen);
            lets.Add(let);
        }

        if (!ResultLines.AllReadBy(condition, lets)) throw new InvalidOperationException(ContinuesItsWhen);
        return lets;
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

    internal void Renders(string elementId)
    {
        if (_rendered.Add(elementId)) return;
        throw new InvalidOperationException(
            $"The plan renders the element '{elementId}' twice, and a page holds one element for an id: render each component once, and an input once for its member.");
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

internal sealed class Branching
{
    private bool _awaitedAfter;

    internal Branching(IfStatement statement, List<Statement> block, bool awaitedBefore, bool armAwaited)
    {
        Outer = statement;
        Last = statement;
        Block = block;
        ElseEntered = awaitedBefore;
        _awaitedAfter = awaitedBefore || armAwaited;
    }

    internal IfStatement Outer { get; }

    internal List<Statement> Block { get; }

    internal IfStatement Last { get; private set; }

    internal bool ElseEntered { get; private set; }

    internal bool Ended { get; private set; }

    internal bool AwaitedAfter => _awaitedAfter;

    internal void ElseIf(List<Statement> alternative, IfStatement statement, bool entered, bool armAwaited)
    {
        Last.Else = alternative;
        Last = statement;
        ElseEntered = entered;
        _awaitedAfter = _awaitedAfter || entered || armAwaited;
    }

    internal void Else(IReadOnlyList<Statement> otherwise, bool awaited)
    {
        Last.Else = otherwise;
        Ended = true;
        _awaitedAfter = _awaitedAfter || awaited;
    }
}
