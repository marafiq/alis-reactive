using System;
using System.Collections.Generic;
using Alis.Reactive.Plan;

namespace Alis.Reactive.Writing;

internal sealed class ResultLines
{
    private readonly Dictionary<string, LetStatement> _results = new(StringComparer.Ordinal);
    private readonly HashSet<string> _undecided = new(StringComparer.Ordinal);

    internal void Written(Statement statement, List<Statement> block)
    {
        if (statement is LetStatement result)
        {
            _results[result.Name] = result;
            _undecided.Add(result.Name);
            return;
        }

        var own = OwnResults(block);
        var reads = new Reads(own);
        References.Of(statement, reads.Read);
        for (var position = own.Count - 1; position >= 0; position--)
        {
            Decide(own[position], reads);
        }

        foreach (var name in reads.Outside)
        {
            AtItsLine(name);
        }
    }

    internal static bool AllReadBy(Expression condition, List<LetStatement> results)
    {
        var read = new HashSet<string>(StringComparer.Ordinal);
        References.Of(condition, whenReached: false, (name, _) => read.Add(name));
        for (var position = results.Count - 1; position >= 0; position--)
        {
            if (!read.Contains(results[position].Name)) return false;
            References.Of(results[position].Expr, whenReached: false, (name, _) => read.Add(name));
        }

        return true;
    }

    private void Decide(LetStatement result, Reads reads)
    {
        _undecided.Remove(result.Name);
        var readOnceWhenReached = reads.OnceWhenReached(result.Name);
        if (readOnceWhenReached && result.Await)
        {
            throw new InvalidOperationException(
                "An async method is called in a term of And or Or that runs only when it is reached, and the pipeline waits for an async method at its line: call it in a When of its own, in the branch where its term is reached.");
        }

        if (readOnceWhenReached) result.Binds = LetBinding.Read;
        References.Of(result.Expr, readOnceWhenReached, reads.Read);
    }

    private void AtItsLine(string name)
    {
        if (!_results.TryGetValue(name, out var result)) return;
        _undecided.Remove(name);
        result.Binds = LetBinding.Line;
    }

    private List<LetStatement> OwnResults(List<Statement> block)
    {
        var own = new List<LetStatement>();
        for (var position = block.Count - 1; position >= 0 && block[position] is LetStatement result && _undecided.Contains(result.Name); position--)
        {
            own.Insert(0, result);
        }

        return own;
    }

    private sealed class Reads
    {
        private readonly HashSet<string> _own = new(StringComparer.Ordinal);
        private readonly HashSet<string> _atOnce = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _whenReached = new(StringComparer.Ordinal);

        internal Reads(List<LetStatement> own)
        {
            foreach (var result in own)
            {
                _own.Add(result.Name);
            }
        }

        internal List<string> Outside { get; } = [];

        internal void Read(string name, bool whenReached)
        {
            if (!_own.Contains(name))
            {
                Outside.Add(name);
                return;
            }

            if (!whenReached)
            {
                _atOnce.Add(name);
                return;
            }

            _whenReached[name] = _whenReached.TryGetValue(name, out var count) ? count + 1 : 1;
        }

        internal bool OnceWhenReached(string name)
            => !_atOnce.Contains(name) && _whenReached.TryGetValue(name, out var count) && count == 1;
    }
}

internal static class References
{
    internal static void Of(Statement statement, Action<string, bool> read)
    {
        switch (statement)
        {
            case SetStatement set:
                Of(set.Target, whenReached: false, read);
                Of(set.Value, whenReached: false, read);
                return;
            case DoStatement step:
                Of(step.Expr, whenReached: false, read);
                return;
            case IfStatement branch:
                Of(branch.When, whenReached: false, read);
                return;
            default:
                throw new InvalidOperationException("A result's line is decided by the statements that read it, and this statement reads none.");
        }
    }

    internal static void Of(Expression expression, bool whenReached, Action<string, bool> read)
    {
        switch (expression)
        {
            case RefExpression reference:
                read(reference.Name, whenReached);
                return;
            case GetExpression get:
                Of(get.Target, whenReached, read);
                return;
            case DataExpression data:
                Of(data.Target, whenReached, read);
                return;
            case CallExpression call:
                Of(call.Target, whenReached, read);
                foreach (var argument in call.Args)
                {
                    Of(argument, whenReached, read);
                }

                return;
            case FnExpression function:
                Of(function.Body, whenReached: false, read);
                return;
            case AnswerExpression answer:
                Of(answer.Body, whenReached: false, read);
                return;
            case OperatorExpression operation:
                Operands(operation, whenReached, read);
                return;
            case LiteralExpression or ObjectExpression:
                return;
            default:
                throw new InvalidOperationException("The plan holds an expression whose reads the writer does not know.");
        }
    }

    private static void Operands(OperatorExpression operation, bool whenReached, Action<string, bool> read)
    {
        var decidedByItsFirstTerm = ReferenceEquals(operation.Op, Operator.And) || ReferenceEquals(operation.Op, Operator.Or);
        for (var position = 0; position < operation.Args.Count; position++)
        {
            Of(operation.Args[position], whenReached || (decidedByItsFirstTerm && position > 0), read);
        }
    }
}
