using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Components;
using Alis.Reactive.Requests;
using Alis.Reactive.Values;
using Microsoft.AspNetCore.Http;

namespace Alis.Reactive.Pipelines;

public sealed class QueryParts<TRequest>
{
    private QueryParts()
    {
    }

    public QueryParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotSupportedException();

    public QueryParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotSupportedException();

    public QueryParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public QueryParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotSupportedException();

    public QueryParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotSupportedException();

    public QueryParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public QueryParts<TRequest> Header(string name, string value) => throw new NotSupportedException();

    public QueryParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value) => throw new NotSupportedException();
}

public sealed class BodyParts<TRequest>
{
    private BodyParts()
    {
    }

    public BodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotSupportedException();

    public BodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotSupportedException();

    public BodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public BodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotSupportedException();

    public BodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotSupportedException();

    public BodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public BodyParts<TRequest> Header(string name, string value) => throw new NotSupportedException();

    public BodyParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value) => throw new NotSupportedException();

    public BodiedParts<TRequest> Json(Action<JsonBody<TRequest>> body) => throw new NotSupportedException();

    public BodiedParts<TRequest> Json(RequestContainer<TRequest> container, Action<JsonBody<TRequest>> body) => throw new NotSupportedException();

    public BodiedParts<TRequest> UrlEncoded(Action<UrlEncodedBody<TRequest>> body) => throw new NotSupportedException();

    public BodiedParts<TRequest> UrlEncoded(RequestForm<TRequest> form, Action<UrlEncodedBody<TRequest>> body) => throw new NotSupportedException();

    public BodiedParts<TRequest> UrlEncoded(RequestContainer<TRequest> container, Action<UrlEncodedBody<TRequest>> body) => throw new NotSupportedException();

    public BodiedParts<TRequest> Multipart(Action<MultipartBody<TRequest>> body) => throw new NotSupportedException();

    public BodiedParts<TRequest> Multipart(RequestForm<TRequest> form, Action<MultipartBody<TRequest>> body) => throw new NotSupportedException();

    public BodiedParts<TRequest> Multipart(RequestContainer<TRequest> container, Action<MultipartBody<TRequest>> body) => throw new NotSupportedException();

    public WholeBodyParts<TRequest> Text(string text) => throw new NotSupportedException();

    public WholeBodyParts<TRequest> Text(ITypedSource<string?> text) => throw new NotSupportedException();

    public WholeBodyParts<TRequest> File(BrowserFile file) => throw new NotSupportedException();
}

public sealed class QueriedParts<TRequest>
{
    private QueriedParts()
    {
    }

    public QueriedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotSupportedException();

    public QueriedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotSupportedException();

    public QueriedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public QueriedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotSupportedException();

    public QueriedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotSupportedException();

    public QueriedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public QueriedParts<TRequest> Header(string name, string value) => throw new NotSupportedException();

    public QueriedParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value) => throw new NotSupportedException();
}

public sealed class WholeBodyParts<TRequest>
{
    private WholeBodyParts()
    {
    }

    public WholeBodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotSupportedException();

    public WholeBodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotSupportedException();

    public WholeBodyParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public WholeBodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotSupportedException();

    public WholeBodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotSupportedException();

    public WholeBodyParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public WholeBodyParts<TRequest> Header(string name, string value) => throw new NotSupportedException();

    public WholeBodyParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value) => throw new NotSupportedException();
}

public sealed class BodiedParts<TRequest>
{
    private BodiedParts()
    {
    }

    public BodiedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotSupportedException();

    public BodiedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotSupportedException();

    public BodiedParts<TRequest> Path<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public BodiedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotSupportedException();

    public BodiedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotSupportedException();

    public BodiedParts<TRequest> Query<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public BodiedParts<TRequest> Header(string name, string value) => throw new NotSupportedException();

    public BodiedParts<TRequest> Header<TValue>(string name, ITypedSource<TValue> value) => throw new NotSupportedException();
}

public sealed class JsonBody<TRequest>
{
    private JsonBody()
    {
    }

    public JsonBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotSupportedException();

    public JsonBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotSupportedException();

    public JsonBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public JsonBody<TRequest> Input<TMember, TValue>(Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        where TValue : TMember
        => throw new NotSupportedException();

    public JsonBody<TRequest> Input<TValue>(Expression<Func<TRequest, TValue?>> member, BoundInput<TValue> input)
        where TValue : struct
        => throw new NotSupportedException();
}

public sealed class UrlEncodedBody<TRequest>
{
    private UrlEncodedBody()
    {
    }

    public UrlEncodedBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotSupportedException();

    public UrlEncodedBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotSupportedException();

    public UrlEncodedBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public UrlEncodedBody<TRequest> Input<TMember, TValue>(Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        where TValue : TMember
        => throw new NotSupportedException();

    public UrlEncodedBody<TRequest> Input<TValue>(Expression<Func<TRequest, TValue?>> member, BoundInput<TValue> input)
        where TValue : struct
        => throw new NotSupportedException();
}

public sealed class MultipartBody<TRequest>
{
    private MultipartBody()
    {
    }

    public MultipartBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, ITypedSource<TValue> value) => throw new NotSupportedException();

    public MultipartBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue>> member, TValue value) => throw new NotSupportedException();

    public MultipartBody<TRequest> Set<TValue>(Expression<Func<TRequest, TValue?>> member, ITypedSource<TValue> value)
        where TValue : struct
        => throw new NotSupportedException();

    public MultipartBody<TRequest> File(Expression<Func<TRequest, IFormFile?>> member, BrowserFile file) => throw new NotSupportedException();

    public MultipartBody<TRequest> File(Expression<Func<TRequest, List<IFormFile>>> member, LiveObjects<BrowserFile> files) => throw new NotSupportedException();

    public MultipartBody<TRequest> Input<TMember, TValue>(Expression<Func<TRequest, TMember>> member, BoundInput<TValue> input)
        where TValue : TMember
        => throw new NotSupportedException();

    public MultipartBody<TRequest> Input<TValue>(Expression<Func<TRequest, TValue?>> member, BoundInput<TValue> input)
        where TValue : struct
        => throw new NotSupportedException();
}
