using System;
using System.Linq.Expressions;
using Alis.Reactive.Requests;
using Alis.Reactive.Values;

namespace Alis.Reactive.Pipelines;

public static class CondensedRequests
{
    public static QueryParts<TRequest> Set<TRequest, TValue>(this QueryParts<TRequest> request, Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value)
        => throw new NotSupportedException();

    public static QueryParts<TRequest> Set<TRequest, TValue>(this QueryParts<TRequest> request, Expression<Func<TRequest, TValue>> member, TValue value)
        => throw new NotSupportedException();

    public static QueryParts<TRequest> Set<TRequest, TValue>(this QueryParts<TRequest> request, Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public static QueryParts<TRequest> Form<TRequest>(this QueryParts<TRequest> request, string formId) => throw new NotSupportedException();

    public static BodyParts<TRequest> Set<TRequest, TValue>(this BodyParts<TRequest> request, Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value)
        => throw new NotSupportedException();

    public static BodyParts<TRequest> Set<TRequest, TValue>(this BodyParts<TRequest> request, Expression<Func<TRequest, TValue>> member, TValue value)
        => throw new NotSupportedException();

    public static BodyParts<TRequest> Set<TRequest, TValue>(this BodyParts<TRequest> request, Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public static BodiedParts<TRequest> Form<TRequest>(this BodyParts<TRequest> request, string formId) => throw new NotSupportedException();

    public static BodiedParts<TRequest> Set<TRequest, TValue>(this BodiedParts<TRequest> request, Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value)
        => throw new NotSupportedException();

    public static BodiedParts<TRequest> Set<TRequest, TValue>(this BodiedParts<TRequest> request, Expression<Func<TRequest, TValue>> member, TValue value)
        => throw new NotSupportedException();

    public static BodiedParts<TRequest> Set<TRequest, TValue>(this BodiedParts<TRequest> request, Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest, Response<TRequest>> OnSuccess<TModel, TRequest>(this GetRequest<TModel, TRequest> request, Action<Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest, Response<TRequest, Markup>> Into<TModel, TRequest>(this GetRequest<TModel, TRequest> request, string elementId)
        => throw new NotSupportedException();

    public static GetRequest<TModel, TRequest, Response<TRequest, ResponseFile>> Download<TModel, TRequest>(this GetRequest<TModel, TRequest> request)
        => throw new NotSupportedException();

    public static HeadRequest<TModel, TRequest, Response<TRequest>> OnSuccess<TModel, TRequest>(this HeadRequest<TModel, TRequest> request, Action<Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public static OptionsRequest<TModel, TRequest, Response<TRequest>> OnSuccess<TModel, TRequest>(this OptionsRequest<TModel, TRequest> request, Action<Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public static OptionsRequest<TModel, TRequest, Response<TRequest, Markup>> Into<TModel, TRequest>(this OptionsRequest<TModel, TRequest> request, string elementId)
        => throw new NotSupportedException();

    public static OptionsRequest<TModel, TRequest, Response<TRequest, ResponseFile>> Download<TModel, TRequest>(this OptionsRequest<TModel, TRequest> request)
        => throw new NotSupportedException();

    public static PostRequest<TModel, TRequest, Response<TRequest>> OnSuccess<TModel, TRequest>(this PostRequest<TModel, TRequest> request, Action<Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public static PostRequest<TModel, TRequest, Response<TRequest, Markup>> Into<TModel, TRequest>(this PostRequest<TModel, TRequest> request, string elementId)
        => throw new NotSupportedException();

    public static PostRequest<TModel, TRequest, Response<TRequest, ResponseFile>> Download<TModel, TRequest>(this PostRequest<TModel, TRequest> request)
        => throw new NotSupportedException();

    public static PutRequest<TModel, TRequest, Response<TRequest>> OnSuccess<TModel, TRequest>(this PutRequest<TModel, TRequest> request, Action<Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public static PutRequest<TModel, TRequest, Response<TRequest, Markup>> Into<TModel, TRequest>(this PutRequest<TModel, TRequest> request, string elementId)
        => throw new NotSupportedException();

    public static PutRequest<TModel, TRequest, Response<TRequest, ResponseFile>> Download<TModel, TRequest>(this PutRequest<TModel, TRequest> request)
        => throw new NotSupportedException();

    public static DeleteRequest<TModel, TRequest, Response<TRequest>> OnSuccess<TModel, TRequest>(this DeleteRequest<TModel, TRequest> request, Action<Pipeline<TModel>> success)
        => throw new NotSupportedException();

    public static DeleteRequest<TModel, TRequest, Response<TRequest, Markup>> Into<TModel, TRequest>(this DeleteRequest<TModel, TRequest> request, string elementId)
        => throw new NotSupportedException();

    public static DeleteRequest<TModel, TRequest, Response<TRequest, ResponseFile>> Download<TModel, TRequest>(this DeleteRequest<TModel, TRequest> request)
        => throw new NotSupportedException();

    public static Notice Notice<TModel>(this Pipeline<TModel> pipeline) => throw new NotSupportedException();
}
