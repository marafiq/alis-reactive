using System;
using System.Collections.Generic;
using System.Globalization;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Execution;

internal sealed class BlockWriter
{
    private readonly Stack<List<Statement>> _open = new();
    private int _results;

    internal BlockWriter(PlanContent content)
    {
        Content = content;
    }

    internal PlanContent Content { get; }

    internal string NextResult() => "result" + (++_results).ToString(CultureInfo.InvariantCulture);

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
