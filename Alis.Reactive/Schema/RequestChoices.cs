using System.Text.Json.Serialization;

namespace Alis.Reactive.Schema;

[JsonConverter(typeof(ChoiceConverter<RequestMethod>))]
internal sealed class RequestMethod : IChoice
{
    internal static readonly RequestMethod Get = new("GET");
    internal static readonly RequestMethod Head = new("HEAD");
    internal static readonly RequestMethod Options = new("OPTIONS");
    internal static readonly RequestMethod Post = new("POST");
    internal static readonly RequestMethod Put = new("PUT");
    internal static readonly RequestMethod Delete = new("DELETE");

    private RequestMethod(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

[JsonConverter(typeof(ChoiceConverter<RequestContract>))]
internal sealed class RequestContract : IChoice
{
    internal static readonly RequestContract Busy = new("busy");
    internal static readonly RequestContract Notice = new("notice");

    private RequestContract(string text)
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

[JsonConverter(typeof(ChoiceConverter<WhenBlank>))]
internal sealed class WhenBlank : IChoice
{
    internal static readonly WhenBlank Send = new("send");
    internal static readonly WhenBlank Leave = new("leave");
    internal static readonly WhenBlank LeaveUnlessAlone = new("leaveUnlessAlone");

    private WhenBlank(string text)
    {
        Text = text;
    }

    public string Text { get; }
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

[JsonConverter(typeof(ChoiceConverter<HttpOutcome>))]
internal sealed class HttpOutcome : IChoice
{
    internal static readonly HttpOutcome Ok = new("ok");
    internal static readonly HttpOutcome Status = new("status");
    internal static readonly HttpOutcome Network = new("network");
    internal static readonly HttpOutcome Redirect = new("redirect");
    internal static readonly HttpOutcome Content = new("content");

    private HttpOutcome(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

[JsonConverter(typeof(ChoiceConverter<BodyFormat>))]
internal sealed class BodyFormat : IChoice
{
    internal static readonly BodyFormat None = new("none");
    internal static readonly BodyFormat Json = new("json");
    internal static readonly BodyFormat UrlEncoded = new("urlEncoded");
    internal static readonly BodyFormat Multipart = new("multipart");
    internal static readonly BodyFormat PlainText = new("text");
    internal static readonly BodyFormat File = new("file");

    private BodyFormat(string text)
    {
        Text = text;
    }

    public string Text { get; }
}
