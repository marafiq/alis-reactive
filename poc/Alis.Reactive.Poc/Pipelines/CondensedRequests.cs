using System;
using System.Linq.Expressions;
using Alis.Reactive.Values;

namespace Alis.Reactive.Pipelines;

public static class CondensedRequests
{
    public static QueryParts<TRequest> Set<TRequest, TValue>(this QueryParts<TRequest> request, Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value)
        => request.Placed(member, request.Form<TValue>(member), Sources.Of(value).Expression);

    public static QueryParts<TRequest> Set<TRequest, TValue>(this QueryParts<TRequest> request, Expression<Func<TRequest, TValue>> member, TValue value)
        => request.Placed(member, request.Form<TValue>(member), request.LiteralOf(value));

    public static QueryParts<TRequest> Set<TRequest, TValue>(this QueryParts<TRequest> request, Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => request.Placed(member, request.Form<TValue?>(member), Sources.Of(value).Expression);
}
