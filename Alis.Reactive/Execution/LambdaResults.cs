using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Execution;

internal sealed class LambdaResults
{
    private readonly List<Result> _results = [];

    internal bool Closed { get; set; }

    internal ResultBinding Add(string name, CallExpression call)
    {
        var binding = new ResultBinding(name, () => Closed);
        _results.Add(new Result(binding, call));
        return binding;
    }

    internal Expression Complete(Expression body)
    {
        for (var index = _results.Count - 1; index >= 0; index--)
        {
            var result = _results[index];
            var reference = result.Binding.Reference;
            var reads = ExpressionReads.In(body).Where(read => read.Reference.Name == reference.Name).ToArray();
            if (reads.Any(read => !ReferenceEquals(read.Reference, reference))) throw ResultBinding.OutsideBlock();
            body = reads is [{ InFunction: false }]
                ? Replace(body, reference, result.Call)
                : new CallExpression([], PathMembers.Required, new FnExpression([reference.Name], body), [result.Call]);
        }

        return body;
    }

    private static Expression Replace(Expression expression, RefExpression reference, Expression value)
    {
        if (ReferenceEquals(expression, reference)) return value;
        return expression switch
        {
            GetExpression get => new GetExpression(get.Path, get.Members, Replace(get.Target, reference, value)),
            DataExpression data => new DataExpression(data.Path, Replace(data.Target, reference, value)),
            AsExpression converted => new AsExpression(Replace(converted.Expr, reference, value), converted.Shape),
            CallExpression call => new CallExpression(call.Path, call.Members, Replace(call.Target, reference, value),
                call.Args.Select(argument => Replace(argument, reference, value)).ToArray()),
            FnExpression function => new FnExpression(function.Params, Replace(function.Body, reference, value)),
            AnswerExpression answer => new AnswerExpression(answer.Arguments, Replace(answer.Body, reference, value)),
            RecordExpression record => new RecordExpression(record.Fields.Select(field => new RecordField(field.Name, Replace(field.Value, reference, value))).ToArray()),
            ListExpression list => new ListExpression(list.Items.Select(item => Replace(item, reference, value)).ToArray()),
            OpExpression operation => new OpExpression(operation.Op, operation.Args.Select(argument => Replace(argument, reference, value)).ToArray()),
            RefExpression or LitExpression or ObjExpression or SignalExpression => expression,
            _ => throw new NotImplementedException(),
        };
    }

    private sealed record Result(ResultBinding Binding, CallExpression Call);
}
