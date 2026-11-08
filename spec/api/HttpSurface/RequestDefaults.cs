using System;
using Alis.Reactive.Requests;

namespace Alis.Reactive.Pipelines;

public enum AntiforgeryIn
{
    Header,
    FormField,
}

public sealed class NoModel
{
    private NoModel()
    {
    }
}

public sealed class RequestDefaults
{
    private RequestDefaults()
    {
    }

    public RequestDefaults Antiforgery(AntiforgeryIn place) => throw new NotSupportedException();

    public RequestDefaults NoAntiforgery() => throw new NotSupportedException();

    public RequestDefaults Header(string name, string value) => throw new NotSupportedException();

    public RequestDefaults ShowBusy() => throw new NotSupportedException();

    public RequestDefaults ShowValidationErrors() => throw new NotSupportedException();

    public RequestDefaults WhileLoading(Action<Pipeline<NoModel>> pipeline) => throw new NotSupportedException();

    public RequestDefaults Finally(Action<Pipeline<NoModel>> pipeline) => throw new NotSupportedException();

    public RequestDefaults OnError<TError>(int status, ErrorContent<TError> content, Action<Response<NoModel, TError>, Pipeline<NoModel>> error)
        => throw new NotSupportedException();

    public RequestDefaults OnError(int status, NoContent content, Action<Response<NoModel>, Pipeline<NoModel>> error)
        => throw new NotSupportedException();

    public RequestDefaults OnError(Action<Failure, Pipeline<NoModel>> failed) => throw new NotSupportedException();

    public RequestDefaults Retry(int attempts, TimeSpan firstWait) => throw new NotSupportedException();
}
