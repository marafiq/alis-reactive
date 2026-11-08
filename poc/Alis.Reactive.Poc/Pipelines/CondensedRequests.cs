using System;
using System.Linq.Expressions;
using Alis.Reactive.Plan;
using Alis.Reactive.Requests;
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

    public static GetRequest<TModel, TRequest, Response<TRequest>> OnSuccess<TModel, TRequest>(this GetRequest<TModel, TRequest> request, Action<Pipeline<TModel>> success)
        => request.Succeeds(BodyKind.None, new Response<TRequest>(), (_, pipeline) => success(pipeline));

    public static GetRequest<TModel, TRequest, Response<TRequest, Markup>> Into<TModel, TRequest>(this GetRequest<TModel, TRequest> request, string elementId)
    {
        Slots.RequireAnId(elementId);
        return request.Succeeds(BodyKind.Html, new Response<TRequest, Markup>(), (reply, loaded) => Slots.Load(loaded.Build, elementId, reply.Body));
    }
}
