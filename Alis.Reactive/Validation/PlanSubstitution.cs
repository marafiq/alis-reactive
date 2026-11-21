using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Validation;

internal static class PlanSubstitution
{
    internal static Expression Applied(FnExpression function, params Expression[] arguments)
    {
        var bound = new Dictionary<string, Expression>(StringComparer.Ordinal);
        for (var at = 0; at < function.Params.Count; at++) bound[function.Params[at]] = arguments[at];
        return Substituted(function.Body, bound);
    }

    private static Expression Substituted(Expression expression, IReadOnlyDictionary<string, Expression> bound) => expression switch
    {
        RefExpression reference => bound.TryGetValue(reference.Name, out var argument) ? argument : reference,
        FnExpression function => new FnExpression(function.Params, Substituted(function.Body, Unbound(bound, function.Params))),
        AnswerExpression answer => new AnswerExpression(answer.Arguments, Substituted(answer.Body, Unbound(bound, [answer.Arguments]))),
        OpExpression operation => new OpExpression(operation.Op, [.. operation.Args.Select(term => Substituted(term, bound))]),
        GetExpression get => new GetExpression(get.Path, get.Members, Substituted(get.Target, bound)),
        DataExpression data => new DataExpression(data.Path, Substituted(data.Target, bound)),
        AsExpression converted => new AsExpression(Substituted(converted.Expr, bound), converted.Shape),
        CallExpression call => new CallExpression(call.Path, call.Members, Substituted(call.Target, bound), [.. call.Args.Select(term => Substituted(term, bound))]),
        RecordExpression record => new RecordExpression([.. record.Fields.Select(field => new RecordField(field.Name, Substituted(field.Value, bound)))]),
        ListExpression list => new ListExpression([.. list.Items.Select(item => Substituted(item, bound))]),
        LitExpression or ObjExpression or SignalExpression => expression,
        _ => throw new InvalidOperationException("A plan expression has no substitution of the names it reads."),
    };

    private static IReadOnlyDictionary<string, Expression> Unbound(IReadOnlyDictionary<string, Expression> bound, IReadOnlyList<string> names)
        => bound.Where(name => !names.Contains(name.Key)).ToDictionary(name => name.Key, name => name.Value, StringComparer.Ordinal);
}
