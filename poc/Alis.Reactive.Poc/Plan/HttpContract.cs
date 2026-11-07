using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Plan;

internal sealed class HttpSend
{
    internal HttpSend(HttpMethod method, IReadOnlyList<PathPart> path, string search, IReadOnlyList<QueryField> query, BodyKind success)
    {
        Method = method;
        Path = path;
        Search = search;
        Query = query;
        Success = success;
    }

    public HttpMethod Method { get; }

    public IReadOnlyList<PathPart> Path { get; }

    public string Search { get; }

    public IReadOnlyList<QueryField> Query { get; }

    public BodyKind Success { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(TextPart), "text")]
[JsonDerivedType(typeof(ValuePart), "value")]
internal abstract class PathPart
{
    private protected PathPart()
    {
    }
}

internal sealed class TextPart : PathPart
{
    internal TextPart(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

internal sealed class ValuePart : PathPart
{
    internal ValuePart(string name, JsonElement value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public JsonElement Value { get; }
}

internal sealed class QueryField
{
    internal QueryField(string name, JsonElement value, WhenNothing whenNothing)
    {
        Name = name;
        Value = value;
        WhenNothing = whenNothing;
    }

    public string Name { get; }

    public JsonElement Value { get; }

    public WhenNothing WhenNothing { get; }
}

[JsonConverter(typeof(ChoiceConverter<WhenNothing>))]
internal sealed class WhenNothing : IChoice
{
    internal static readonly WhenNothing Leave = new("leave");
    internal static readonly WhenNothing Empty = new("empty");

    private WhenNothing(string text)
    {
        Text = text;
    }

    public string Text { get; }
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
    internal static readonly BodyKind Json = new("json");

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
