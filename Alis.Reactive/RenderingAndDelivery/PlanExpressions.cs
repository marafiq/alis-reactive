using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Schema;

namespace Alis.Reactive.RenderingAndDelivery;

internal static class PlanExpressions
{
    internal static IEnumerable<Expression> Written(IReadOnlyList<Statement> block) => block.SelectMany(Written);

    internal static IEnumerable<Expression> Within(Expression expression) => [expression, .. Parts(expression).SelectMany(Within)];

    internal static IEnumerable<Expression> Parts(Expression expression) => expression switch
    {
        GetExpression read => [read.Target],
        DataExpression data => [data.Target],
        AsExpression shaped => [shaped.Expr],
        CallExpression call => [call.Target, .. call.Args],
        FnExpression function => [function.Body],
        AnswerExpression answer => [answer.Body],
        RecordExpression record => record.Fields.Select(field => field.Value),
        ListExpression list => list.Items,
        OpExpression operation => operation.Args,
        LitExpression or ObjExpression or RefExpression or SignalExpression => [],
        _ => throw new InvalidOperationException("A plan expression has no walk of the expressions it holds."),
    };

    private static IEnumerable<Expression> Written(Statement statement) => statement switch
    {
        SetStatement set => [set.Target, set.Value],
        DoStatement step => [step.Expr],
        LetStatement binding => [binding.Expr],
        IfStatement branch => [branch.When, .. Written(branch.Then), .. Written(branch.Else)],
        AllStatement parallel => parallel.Blocks.SelectMany(Written),
        RequestStatement request =>
        [
            request.Send, .. Written(request.Check),
            .. request.Layers.SelectMany(layer => Written(layer.WhileLoading).Concat(Written(layer.Finally))),
            .. Written(request.Success.Do), .. request.Statuses.SelectMany(status => Written(status.Do)),
            .. Written(request.Otherwise.Do), .. Written(request.Chained),
        ],
        ListenStatement listen => [listen.Object, .. Written(listen.Do)],
        StopStatement => [],
        _ => throw new InvalidOperationException("A plan statement has no walk of the expressions it holds."),
    };
}
