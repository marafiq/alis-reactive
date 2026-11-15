using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.RequestsAndResponses;

internal sealed class HttpResponse
{
    internal HttpResponse(int status, HttpOutcome outcome, BrowserHeaders headers, EvaluatedValue request, IReadOnlyList<SentInput> inputs, ResponseBody body)
    {
        Status = status;
        Outcome = outcome;
        Headers = headers;
        Request = request;
        Inputs = inputs;
        Body = body;
    }

    public int Status { get; }

    public HttpOutcome Outcome { get; }

    public BrowserHeaders Headers { get; }

    public EvaluatedValue Request { get; }

    public IReadOnlyList<SentInput> Inputs { get; }

    public ResponseBody Body { get; }
}

internal sealed class BrowserHeaders
{
    internal BrowserHeaders()
    {
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Kind))]
[JsonDerivedType(typeof(NoneResponseBody))]
[JsonDerivedType(typeof(JsonResponseBody))]
[JsonDerivedType(typeof(TextResponseBody))]
[JsonDerivedType(typeof(HtmlResponseBody))]
[JsonDerivedType(typeof(FileResponseBody))]
internal abstract class ResponseBody
{
    private protected ResponseBody()
    {
    }

    public abstract string Kind { get; }
}

internal sealed class NoneResponseBody : ResponseBody
{
    internal NoneResponseBody()
    {
    }

    public override string Kind => "none";
}

internal sealed class JsonResponseBody : ResponseBody
{
    internal JsonResponseBody(EvaluatedValue value)
    {
        Value = value;
    }

    public override string Kind => "json";

    public EvaluatedValue Value { get; }
}

internal sealed class TextResponseBody : ResponseBody
{
    internal TextResponseBody(string value)
    {
        Value = value;
    }

    public override string Kind => "text";

    public string Value { get; }
}

internal sealed class HtmlResponseBody : ResponseBody
{
    internal HtmlResponseBody(string value)
    {
        Value = value;
    }

    public override string Kind => "html";

    public string Value { get; }
}

internal sealed class FileResponseBody : ResponseBody
{
    internal FileResponseBody(ResponseFile value)
    {
        Value = value;
    }

    public override string Kind => "file";

    public ResponseFile Value { get; }
}
