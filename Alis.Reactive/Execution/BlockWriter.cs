using System;
using System.Collections.Generic;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Execution;

internal sealed class BlockWriter
{
    private readonly Stack<List<Statement>> _open = new();

    internal BlockWriter(PlanContent content)
    {
        Content = content;
    }

    internal PlanContent Content { get; }

    internal void Write(Statement statement)
    {
        if (_open.Count == 0)
            throw new InvalidOperationException("A step is written inside a pipeline, the callback its trigger runs: no pipeline is open here.");
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
}
