using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Components;
using Alis.Reactive.Values;

namespace Alis.Reactive.Requests;

public sealed class ResponseHeaders : JsObject
{
    private ResponseHeaders()
    {
    }

    public MethodResult<string?> Get(string name) => throw new NotSupportedException();
}

public sealed class ResponseFile
{
    public string Name { get; init; } = "";

    public long Size { get; init; }

    public string Type { get; init; } = "";
}

[Mirrors("lib.dom", "File")]
public sealed class BrowserFile : JsObject
{
    public ReadOnly<string> Name => throw new NotSupportedException();

    public ReadOnly<long> Size => throw new NotSupportedException();

    public ReadOnly<string> Type => throw new NotSupportedException();

    public ReadOnly<long> LastModified => throw new NotSupportedException();
}

public sealed class PageUrl<TPage> : JsObject
{
    private PageUrl()
    {
    }

    public TypedSource<TValue> Read<TValue>(Expression<Func<TPage, TValue>> member) => throw new NotSupportedException();
}

public sealed class ValidationProblem
{
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors { get; init; } = new Dictionary<string, IReadOnlyList<string>>();
}

public class RequestContainer<TModel>
{
    private protected RequestContainer()
    {
    }
}

public sealed class RequestForm<TModel> : RequestContainer<TModel>
{
    private RequestForm()
    {
    }
}

public sealed class Notice
{
    private Notice()
    {
    }

    public void Success(string text) => throw new NotSupportedException();

    public void Success(ITypedSource<string?> text) => throw new NotSupportedException();

    public void Error(string text) => throw new NotSupportedException();

    public void Error(ITypedSource<string?> text) => throw new NotSupportedException();

    public void Warning(string text) => throw new NotSupportedException();

    public void Warning(ITypedSource<string?> text) => throw new NotSupportedException();

    public void Info(string text) => throw new NotSupportedException();

    public void Info(ITypedSource<string?> text) => throw new NotSupportedException();
}
