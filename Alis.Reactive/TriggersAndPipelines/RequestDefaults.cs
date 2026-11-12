using System;
using Alis.Reactive.RequestsAndResponses;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// Where a request carries the antiforgery token.
/// </summary>
public sealed class AntiforgeryIn
{
    /// <summary>The header <c>RequestVerificationToken</c>, sent with every body.</summary>
    public static readonly AntiforgeryIn Header = new();

    /// <summary>The field <c>__RequestVerificationToken</c> of a URL-encoded or multipart body.</summary>
    public static readonly AntiforgeryIn FormField = new();

    private AntiforgeryIn()
    {
    }
}

/// <summary>
/// What every request of the app takes unless it says otherwise, configured once at startup.
/// </summary>
/// <remarks>
/// A default's pipeline names no member of a view, since no page exists at startup. A request takes none of the
/// defaults with <c>NoDefaults()</c>; the antiforgery choice holds for every POST, PUT and DELETE to the page's origin.
/// </remarks>
public sealed class RequestDefaults
{
    private RequestDefaults()
    {
    }

    /// <summary>Sends the host's antiforgery token with every POST, PUT and DELETE to the page's origin.</summary>
    /// <param name="place">Where the token is sent: the header or the form field.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults Antiforgery(AntiforgeryIn place) => throw new NotImplementedException();

    /// <summary>Sends no antiforgery token: the app's endpoints do not ask for one.</summary>
    /// <returns>The defaults.</returns>
    public RequestDefaults NoAntiforgery() => throw new NotImplementedException();

    /// <summary>Sends a header with a fixed text on every request; a request's own header of the same name replaces it.</summary>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The header's text.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults Header(string name, string value) => throw new NotImplementedException();

    /// <summary>Shows the page's busy indicator on what a request updates, from its start to its end.</summary>
    /// <returns>The defaults.</returns>
    public RequestDefaults ShowBusy() => throw new NotImplementedException();

    /// <summary>Shows a server's validation errors, the body of a 400 answer, as the browser shows its own.</summary>
    /// <returns>The defaults.</returns>
    public RequestDefaults ShowValidationErrors() => throw new NotImplementedException();

    /// <summary>Runs a pipeline when a request starts, before the request's own <c>WhileLoading</c>.</summary>
    /// <param name="pipeline">The steps to run.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults WhileLoading(Action<Pipeline<NoModel>> pipeline) => throw new NotImplementedException();

    /// <summary>Runs a pipeline after a request's route, after the request's own <c>Finally</c>.</summary>
    /// <param name="pipeline">The steps to run.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults Finally(Action<Pipeline<NoModel>> pipeline) => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers a request with one status and the request names no route for it, reading the body as the kind named.</summary>
    /// <param name="status">The status the route is for, from 300 to 599.</param>
    /// <param name="content">The kind the answer's body is read as.</param>
    /// <param name="error">The route's pipeline, with the reply.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults OnError<TError>(int status, ErrorContent<TError> content, Action<Response<NoModel, TError>, Pipeline<NoModel>> error)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline when the server answers a request with one status and the request names no route for it, without reading the body.</summary>
    /// <param name="status">The status the route is for, from 300 to 599.</param>
    /// <param name="content">The kind of a route with no data.</param>
    /// <param name="error">The route's pipeline, with the reply.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults OnError(int status, NoContent content, Action<Response<NoModel>, Pipeline<NoModel>> error)
        => throw new NotImplementedException();

    /// <summary>Runs a pipeline for every outcome of a request that neither it nor a default route takes.</summary>
    /// <param name="failed">The route's pipeline, with what is known of the failure.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults OnError(Action<Failure, Pipeline<NoModel>> failed) => throw new NotImplementedException();

    /// <summary>Runs a pipeline for every outcome of a request that neither it nor a default route takes, and reads the JSON body of an answer whose status is not 2xx as <typeparamref name="TError"/>.</summary>
    /// <param name="failed">The route's pipeline, with what is known of the failure.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults OnError<TError>(Action<Failure<TError>, Pipeline<NoModel>> failed) => throw new NotImplementedException();

    /// <summary>Sets the numbers of the one retry policy: a GET, HEAD, OPTIONS, PUT or DELETE is sent again after a network failure and after a 408, 429, 502, 503 or 504.</summary>
    /// <remarks>A POST is never sent again, and a request opts out with <c>NoRetry()</c>.</remarks>
    /// <param name="attempts">How many times a request is sent again after its first attempt.</param>
    /// <param name="firstWait">The wait before the first retry; each later wait is longer.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults Retry(int attempts, TimeSpan firstWait) => throw new NotImplementedException();
}
