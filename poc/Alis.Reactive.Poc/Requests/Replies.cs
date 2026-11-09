using System.Text.Json.Serialization;
using Alis.Reactive.Components;
using Alis.Reactive.Plan;

namespace Alis.Reactive.Requests;

public class Response<TRequest> : JsObject
{
    internal Response()
    {
    }

    public ReadOnly<int> Status => ReadOnly<int>();

    public ResponseHeaders Headers => LiveObject(new ResponseHeaders());
}

public sealed class Response<TRequest, TBody> : Response<TRequest>
{
    internal Response()
    {
    }

    public ReadOnly<TBody> Body => ReadOnly<TBody>();
}

public sealed class ResponseHeaders : JsObject
{
    internal ResponseHeaders()
    {
    }

    public MethodResult<string?> Get(string name) => Method<string?>([Fixed(HeaderNames.Readable(name))]);
}

public sealed class Markup
{
    private Markup()
    {
    }
}

[JsonConverter(typeof(JsonStringEnumConverter<FailureOutcome>))]
public enum FailureOutcome
{
    [JsonStringEnumMemberName(HttpOutcome.StatusText)]
    Status,

    [JsonStringEnumMemberName(HttpOutcome.NetworkText)]
    Network,

    [JsonStringEnumMemberName(HttpOutcome.RedirectText)]
    Redirect,

    [JsonStringEnumMemberName(HttpOutcome.ContentText)]
    Content,
}

public sealed class Failure : JsObject
{
    internal Failure()
    {
    }

    public ReadOnly<FailureOutcome> Outcome => ReadOnly<FailureOutcome>();

    public ReadOnly<int> Status => ReadOnly<int>();

    public ResponseHeaders Headers => LiveObject(new ResponseHeaders());
}
