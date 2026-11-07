using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Plan;

internal sealed class HttpSend
{
    internal HttpSend(HttpMethod method, string url, BodyKind success)
    {
        Method = method;
        Url = url;
        Success = success;
    }

    public HttpMethod Method { get; }

    public string Url { get; }

    public BodyKind Success { get; }
}

internal sealed class HttpResponse
{
    private HttpResponse(bool ok, int status, HttpOutcome outcome, JsonElement body)
    {
        Ok = ok;
        Status = status;
        Outcome = outcome;
        Body = body;
    }

    public bool Ok { get; }

    public int Status { get; }

    public HttpOutcome Outcome { get; }

    public JsonElement Body { get; }
}

[JsonConverter(typeof(ChoiceConverter<HttpMethod>))]
internal sealed class HttpMethod : IChoice
{
    internal static readonly HttpMethod Get = new("GET");

    private HttpMethod(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

[JsonConverter(typeof(ChoiceConverter<BodyKind>))]
internal sealed class BodyKind : IChoice
{
    internal static readonly BodyKind Html = new("html");

    private BodyKind(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

[JsonConverter(typeof(ChoiceConverter<HttpOutcome>))]
internal sealed class HttpOutcome : IChoice
{
    internal static readonly HttpOutcome Success = new("success");
    internal static readonly HttpOutcome Status = new("status");
    internal static readonly HttpOutcome Network = new("network");
    internal static readonly HttpOutcome Redirect = new("redirect");
    internal static readonly HttpOutcome Kind = new("kind");

    private HttpOutcome(string text)
    {
        Text = text;
    }

    public string Text { get; }
}
