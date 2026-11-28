using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Schema;

internal sealed class Layer
{
    internal Layer(IReadOnlyList<Statement> whileLoading, IReadOnlyList<Statement> @finally)
    {
        WhileLoading = whileLoading;
        Finally = @finally;
    }

    public IReadOnlyList<Statement> WhileLoading { get; }

    public IReadOnlyList<Statement> Finally { get; }
}

internal sealed class Route
{
    internal Route(SuccessBody kind, IReadOnlyList<Statement> body)
    {
        Kind = kind;
        Do = body;
    }

    public SuccessBody Kind { get; }

    public IReadOnlyList<Statement> Do { get; }
}

[JsonConverter(typeof(ChoiceConverter<SuccessBody>))]
internal sealed class SuccessBody : IChoice
{
    internal static readonly SuccessBody None = new("none");
    internal static readonly SuccessBody Json = new("json");
    internal static readonly SuccessBody PlainText = new("text");
    internal static readonly SuccessBody Html = new("html");
    internal static readonly SuccessBody File = new("file");

    private SuccessBody(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

internal sealed class StatusRoute
{
    internal StatusRoute(int status, ErrorBody kind, IReadOnlyList<Statement> body)
    {
        Status = status;
        Kind = kind;
        Do = body;
    }

    public int Status { get; }

    public ErrorBody Kind { get; }

    public IReadOnlyList<Statement> Do { get; }
}

[JsonConverter(typeof(ChoiceConverter<ErrorBody>))]
internal sealed class ErrorBody : IChoice
{
    internal static readonly ErrorBody None = new("none");
    internal static readonly ErrorBody Json = new("json");
    internal static readonly ErrorBody PlainText = new("text");
    internal static readonly ErrorBody Html = new("html");

    private ErrorBody(string text)
    {
        Text = text;
    }

    public string Text { get; }
}

internal sealed class OtherwiseRoute
{
    internal OtherwiseRoute(OtherwiseBody kind, IReadOnlyList<Statement> body)
    {
        Kind = kind;
        Do = body;
    }

    public OtherwiseBody Kind { get; }

    public IReadOnlyList<Statement> Do { get; }
}

[JsonConverter(typeof(ChoiceConverter<OtherwiseBody>))]
internal sealed class OtherwiseBody : IChoice
{
    internal static readonly OtherwiseBody None = new("none");
    internal static readonly OtherwiseBody Json = new("json");

    private OtherwiseBody(string text)
    {
        Text = text;
    }

    public string Text { get; }
}
