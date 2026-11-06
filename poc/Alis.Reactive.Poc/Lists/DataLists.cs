using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Plan;
using Alis.Reactive.Types;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Lists;

public static class DataLists
{
    public static TypedSource<int> Count<TItem>(this TypedSource<IReadOnlyList<TItem>> list)
        => new DataRead<int>(ListForms.Length(list.Expression), list.Build);

    public static TypedSource<IReadOnlyList<TItem>> Where<TItem>(this TypedSource<IReadOnlyList<TItem>> list, Expression<Func<TItem, bool>> keep)
        => new DataRead<IReadOnlyList<TItem>>(ListForms.Filter(list.Expression, ItemRead(list.Build, keep)), list.Build);

    public static TypedSource<IReadOnlyList<TResult>> Select<TItem, TResult>(this TypedSource<IReadOnlyList<TItem>> list, Expression<Func<TItem, TResult>> field)
        => new DataRead<IReadOnlyList<TResult>>(ListForms.Map(list.Expression, ItemRead(list.Build, field)), list.Build);

    private static FnExpression ItemRead(PlanBuild build, LambdaExpression read)
    {
        var item = build.ItemName();
        var steps = JsonPath.Of(read, build.Values);
        return new FnExpression([item], new GetExpression(new RefExpression(item), steps, ReadPresence.Optional));
    }
}
