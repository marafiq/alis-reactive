using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Execution;

internal static class ExpressionReads
{
    internal static IEnumerable<Read> In(Expression expression, bool later = false, bool inFunction = false)
    {
        if (expression is RefExpression reference)
        {
            yield return new Read(reference, later, inFunction);
            yield break;
        }

        if (expression is FnExpression function)
        {
            foreach (var read in In(function.Body, later, true))
            {
                if (!function.Params.Contains(read.Reference.Name)) yield return read;
            }

            yield break;
        }

        if (expression is OpExpression operation)
        {
            for (var index = 0; index < operation.Args.Count; index++)
            {
                var reachedLater = later || index > 0 &&
                    (operation.Op == Operator.And || operation.Op == Operator.Or || operation.Op == Operator.Coalesce || operation.Op == Operator.Cond);
                foreach (var read in In(operation.Args[index], reachedLater, inFunction)) yield return read;
            }

            yield break;
        }

        var children = expression switch
        {
            GetExpression get => new[] { get.Target },
            DataExpression data => new[] { data.Target },
            AsExpression converted => new[] { converted.Expr },
            CallExpression call => CallParts(call),
            AnswerExpression answer => new[] { answer.Body },
            RecordExpression record => Fields(record),
            ListExpression list => list.Items,
            LitExpression or ObjExpression or SignalExpression => Array.Empty<Expression>(),
            _ => throw new InvalidOperationException("A plan expression has no walk of the references it reads."),
        };
        foreach (var child in children)
        {
            foreach (var read in In(child, later, inFunction || expression is FnExpression or AnswerExpression)) yield return read;
        }
    }

    private static IEnumerable<Expression> CallParts(CallExpression call)
    {
        yield return call.Target;
        foreach (var argument in call.Args) yield return argument;
    }

    private static IEnumerable<Expression> Fields(RecordExpression record)
    {
        foreach (var field in record.Fields) yield return field.Value;
    }

    internal sealed record Read(RefExpression Reference, bool Later, bool InFunction);
}
