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

[JsonConverter(typeof(ChoiceConverter<FailureOutcome>))]
public sealed class FailureOutcome : IChoice
{
    public static readonly FailureOutcome Status = new(HttpOutcome.Status);
    public static readonly FailureOutcome Network = new(HttpOutcome.Network);
    public static readonly FailureOutcome Redirect = new(HttpOutcome.Redirect);
    public static readonly FailureOutcome Content = new(HttpOutcome.Content);

    private readonly HttpOutcome _outcome;

    private FailureOutcome(HttpOutcome outcome)
    {
        _outcome = outcome;
    }

    string IChoice.Text => _outcome.Text;
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
