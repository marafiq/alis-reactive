using System;
using Alis.Reactive.Plan;
using Alis.Reactive.Types;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Components;

public sealed class MethodResult<TResult> : TypedSource<TResult>
{
    private readonly Expression _read;

    internal MethodResult(Expression read, PlanBuild build)
    {
        _read = read;
        Build = build;
    }

    internal override Expression Expression => Build.InScope(_read);

    internal override PlanBuild Build { get; }

    public TypedSource<TRead> Read<TRead>(System.Linq.Expressions.Expression<Func<TResult, TRead>> path)
        => new DataRead<TRead>(new DataExpression(Expression, JsonPath.Of(path, Build.Starts, nameof(path))), Build);
}
