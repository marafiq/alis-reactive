using System;
using System.Collections.Generic;
using System.Globalization;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Execution;

internal sealed class BlockWriter
{
    private readonly Stack<List<Statement>> _open = new();
    private readonly Stack<HandlerResults> _handlers = new();
    private int _results;

    internal BlockWriter(PlanContent content)
    {
        Content = content;
    }

    internal PlanContent Content { get; }

    internal ResultBinding Result(CallExpression call, bool awaits)
    {
        var name = ResultBinding.NameStem + (++_results).ToString(CultureInfo.InvariantCulture);
        Write(new LetStatement(name, awaits, LetBinds.Line, call));
        return _handlers.Peek().Add(name);
    }

    internal IReadOnlyList<Statement> Handler(Action body)
    {
        var handler = new HandlerResults();
        _handlers.Push(handler);
        try
        {
            return handler.Complete(Block(body));
        }
        finally
        {
            handler.Closed = true;
            _handlers.Pop();
        }
    }

    internal IReadOnlyList<Statement> TakeResults(Place place, Expression condition)
    {
        if (_open.Count == 0 || !ReferenceEquals(_open.Peek(), place.Block)) throw BrokenSentence();
        var suffix = place.Block.GetRange(place.Index + 1, place.Block.Count - place.Index - 1);
        var needed = new HashSet<RefExpression>();
        foreach (var read in ExpressionReads.In(condition)) needed.Add(read.Reference);
        var handler = _handlers.Peek();
        for (var index = suffix.Count - 1; index >= 0; index--)
        {
            if (suffix[index] is not LetStatement result || !handler.IsResult(result) || !needed.Contains(handler.ReferenceOf(result)))
                throw BrokenSentence();
            foreach (var read in ExpressionReads.In(result.Expr)) needed.Add(read.Reference);
        }

        foreach (var statement in suffix) handler.MovedToElseIf((LetStatement)statement);
        place.Block.RemoveRange(place.Index + 1, suffix.Count);
        return suffix;
    }

    internal static InvalidOperationException BrokenSentence()
        => new("An ElseIf or an Else continues the When it follows, before any other step of its block: write it in the same sentence.");

    internal void Write(Statement statement)
    {
        if (_open.Count == 0)
            throw new InvalidOperationException("A step is written inside a pipeline, the callback its trigger runs: no pipeline is open here.");
        _open.Peek().Add(statement);
    }

    internal Place Placed(Statement statement)
    {
        Write(statement);
        var block = _open.Peek();
        return new Place(block, block.Count - 1);
    }

    internal bool Replaced(Place place, Statement statement)
    {
        var stillItsBlocksLastStep = _open.Count > 0 && ReferenceEquals(_open.Peek(), place.Block) && place.Index == place.Block.Count - 1;
        if (!stillItsBlocksLastStep) return false;
        place.Block[place.Index] = statement;
        return true;
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

    internal sealed record Place(List<Statement> Block, int Index);
}
