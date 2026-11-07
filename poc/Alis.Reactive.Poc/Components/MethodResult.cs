using System;
using Alis.Reactive.Plan;
using Alis.Reactive.Types;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Components;

public sealed class MethodResult<TResult> : TypedSource<TResult>
{
    internal MethodResult(Expression read, PlanBuild build)
    {
        Expression = read;
        Build = build;
    }

    internal override Expression Expression { get; }

    internal override PlanBuild Build { get; }

    public TypedSource<TRead> Read<TRead>(System.Linq.Expressions.Expression<Func<TResult, TRead>> path)
        => new DataRead<TRead>(new DataExpression(Expression, JsonPath.Of(path, Build.Starts)), Build);
}
