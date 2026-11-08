using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Plan;

internal sealed class HttpSend
{
    internal HttpSend(HttpMethod method, IReadOnlyList<PathPart> path, string search, IReadOnlyList<SentField> query, IReadOnlyList<SentHeader> headers, BodyKind success)
    {
        Method = method;
        Path = path;
        Search = search;
        Query = query;
        Headers = headers;
        Success = success;
    }

    public HttpMethod Method { get; }

    public IReadOnlyList<PathPart> Path { get; }

    public string Search { get; }

    public IReadOnlyList<SentField> Query { get; }

    public IReadOnlyList<SentHeader> Headers { get; }

    public BodyKind Success { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = Discriminator)]
[JsonDerivedType(typeof(TextPart), TextKind)]
[JsonDerivedType(typeof(ValuePart), ValueKind)]
internal abstract class PathPart
{
    internal const string Discriminator = "kind";
    internal const string TextKind = "text";
    internal const string ValueKind = "value";

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
    internal ValuePart(string name, JsonElement value, WireForm form)
    {
        Name = name;
        Value = value;
        Form = form;
    }

    public string Name { get; }

    public JsonElement Value { get; }

    public WireForm Form { get; }
}

internal sealed class SentField
{
    internal SentField(string name, JsonElement value, WireForm form, WhenNothing whenNothing)
    {
        Name = name;
        Value = value;
        Form = form;
        WhenNothing = whenNothing;
    }

    public string Name { get; }

    public JsonElement Value { get; }

    public WireForm Form { get; }

    public WhenNothing WhenNothing { get; }
}

internal sealed class SentHeader
{
    internal SentHeader(string name, JsonElement value, WireForm form)
    {
        Name = name;
        Value = value;
        Form = form;
    }

    public string Name { get; }

    public JsonElement Value { get; }

    public WireForm Form { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(TextForm), "text")]
[JsonDerivedType(typeof(NumberForm), "number")]
[JsonDerivedType(typeof(BooleanForm), "boolean")]
[JsonDerivedType(typeof(DateForm), "date")]
[JsonDerivedType(typeof(InstantForm), "instant")]
[JsonDerivedType(typeof(WrittenForm), "written")]
[JsonDerivedType(typeof(ListForm), "list")]
internal abstract class WireForm
{
    internal static readonly WireForm Text = new TextForm();
    internal static readonly WireForm Number = new NumberForm();
    internal static readonly WireForm Boolean = new BooleanForm();
    internal static readonly WireForm Date = new DateForm();
    internal static readonly WireForm Instant = new InstantForm();
    internal static readonly WireForm Written = new WrittenForm();

    private protected WireForm()
    {
    }
}

internal sealed class TextForm : WireForm
{
    internal TextForm()
    {
    }
}

internal sealed class NumberForm : WireForm
{
    internal NumberForm()
    {
    }
}

internal sealed class BooleanForm : WireForm
{
    internal BooleanForm()
    {
    }
}

internal sealed class DateForm : WireForm
{
    internal DateForm()
    {
    }
}

internal sealed class InstantForm : WireForm
{
    internal InstantForm()
    {
    }
}

internal sealed class WrittenForm : WireForm
{
    internal WrittenForm()
    {
    }
}

internal sealed class ListForm : WireForm
{
    internal ListForm(ScalarKind item, ItemWhenNothing itemWhenNothing)
    {
        Item = item;
        ItemWhenNothing = itemWhenNothing;
    }

    public ScalarKind Item { get; }

    public ItemWhenNothing ItemWhenNothing { get; }
}

[JsonConverter(typeof(ChoiceConverter<ItemWhenNothing>))]
internal sealed class ItemWhenNothing : IChoice
{
    internal static readonly ItemWhenNothing Empty = new("empty");
    internal static readonly ItemWhenNothing Stop = new("stop");

    private ItemWhenNothing(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

[JsonConverter(typeof(ChoiceConverter<ScalarKind>))]
internal sealed class ScalarKind : IChoice
{
    internal static readonly ScalarKind PlainText = new("text");
    internal static readonly ScalarKind Number = new("number");
    internal static readonly ScalarKind Boolean = new("boolean");
    internal static readonly ScalarKind Date = new("date");
    internal static readonly ScalarKind Instant = new("instant");
    internal static readonly ScalarKind Written = new("written");

    private ScalarKind(string text)
    {
        Text = text;
    }

    public string Text { get; }
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
