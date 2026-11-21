using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Declarations;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Schema;

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
    private const int StandardAttempts = 3;
    private const int StandardFirstWaitMilliseconds = 2000;
    private const int ValidationErrorsStatus = 400;

    private static readonly TimeSpan RetriesEnd = TimeSpan.FromSeconds(30);

    private readonly List<(string Name, string Value)> _headers = [];
    private readonly List<DefaultStatus> _statuses = [];
    private readonly List<DefaultOtherwise> _otherwise = [];
    private bool _hasLoading;
    private bool _hasFinally;
    private bool _retrySet;

    private RequestDefaults()
    {
    }

    internal AntiforgeryChoice AntiforgeryChoice { get; private set; } = AntiforgeryChoice.NotChosen;

    internal IReadOnlyList<(string Name, string Value)> Headers => _headers;

    internal bool ShowsBusy { get; private set; }

    internal Action<Pipeline<NoModel>> LoadingBlock { get; private set; } = NoSteps;

    internal Action<Pipeline<NoModel>> FinallyBlock { get; private set; } = NoSteps;

    internal IReadOnlyList<DefaultStatus> Statuses => _statuses;

    internal IReadOnlyList<DefaultOtherwise> CatchAll => _otherwise;

    internal int Attempts { get; private set; } = StandardAttempts;

    internal int FirstWaitMilliseconds { get; private set; } = StandardFirstWaitMilliseconds;

    internal static RequestDefaults Configured(Action<RequestDefaults> requests)
    {
        var defaults = new RequestDefaults();
        requests(defaults);
        return defaults;
    }

    /// <summary>Sends the host's antiforgery token with every POST, PUT and DELETE to the page's origin.</summary>
    /// <param name="place">Where the token is sent: the header or the form field.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults Antiforgery(AntiforgeryIn place)
        => Chosen((place ?? throw new ArgumentNullException(nameof(place))) == AntiforgeryIn.Header ? AntiforgeryChoice.Header : AntiforgeryChoice.Field);

    /// <summary>Sends no antiforgery token: the app's endpoints do not ask for one.</summary>
    /// <returns>The defaults.</returns>
    public RequestDefaults NoAntiforgery() => Chosen(AntiforgeryChoice.NoToken);

    /// <summary>Sends a header with a fixed text on every request; a request's own header of the same name replaces it.</summary>
    /// <remarks>A name given twice, ignoring case, and a name a request may not send are refused at startup.</remarks>
    /// <param name="name">The header's name.</param>
    /// <param name="value">The header's text.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults Header(string name, string value)
    {
        if (name is null) throw new ArgumentNullException(nameof(name));
        if (value is null) throw new ArgumentNullException(nameof(value));
        HeaderNames.Request(name);
        HeaderNames.Value(name, value);
        if (_headers.Any(header => header.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Header '{name}' is supplied twice; send each header name once, ignoring case.");
        _headers.Add((name, value));
        return this;
    }

    /// <summary>Shows the page's busy indicator on what a request updates, from its start to its end.</summary>
    /// <remarks>
    /// The indicator shows on each element and component the request's success route writes outside a condition, and
    /// on the page when it writes none. A request names its own elements with <c>Busy</c>, or shows none with <c>NoBusy()</c>.
    /// </remarks>
    /// <returns>The defaults.</returns>
    public RequestDefaults ShowBusy()
    {
        if (ShowsBusy) throw new InvalidOperationException("AddReactiveRequests shows the busy indicator once; call ShowBusy() once.");
        ShowsBusy = true;
        return this;
    }

    /// <summary>Shows a server's validation errors, the body of a 400 answer, as the browser shows its own.</summary>
    /// <remarks>It is the default route for status 400, which a request's own route for 400 replaces.</remarks>
    /// <returns>The defaults.</returns>
    public RequestDefaults ShowValidationErrors()
        => Status(ValidationErrorsStatus, ErrorBody.Json,
            routes => routes.Route(new Response<NoModel, ValidationProblem>(SuccessBody.Json), (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body)));

    /// <summary>Runs a pipeline when a request starts, before the request's own <c>WhileLoading</c>.</summary>
    /// <param name="pipeline">The steps to run.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults WhileLoading(Action<Pipeline<NoModel>> pipeline)
    {
        if (pipeline is null) throw new ArgumentNullException(nameof(pipeline));
        if (_hasLoading) throw new InvalidOperationException("AddReactiveRequests already has a WhileLoading; put its steps in one block.");
        _hasLoading = true;
        LoadingBlock = pipeline;
        return this;
    }

    /// <summary>Runs a pipeline after a request's route, after the request's own <c>Finally</c>.</summary>
    /// <param name="pipeline">The steps to run.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults Finally(Action<Pipeline<NoModel>> pipeline)
    {
        if (pipeline is null) throw new ArgumentNullException(nameof(pipeline));
        if (_hasFinally) throw new InvalidOperationException("AddReactiveRequests already has a Finally; put its steps in one block.");
        _hasFinally = true;
        FinallyBlock = pipeline;
        return this;
    }

    /// <summary>Runs a pipeline when the server answers a request with one status and the request names no route for it, reading the body as the kind named.</summary>
    /// <param name="status">The status the route is for, from 300 to 599.</param>
    /// <param name="content">The kind the answer's body is read as.</param>
    /// <param name="error">The route's pipeline, with the reply.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults OnError<TError>(int status, ErrorContent<TError> content, Action<Response<NoModel, TError>, Pipeline<NoModel>> error)
    {
        if (content is null) throw new ArgumentNullException(nameof(content));
        if (error is null) throw new ArgumentNullException(nameof(error));
        return Status(status, content.ErrorKind, routes => routes.Route(new Response<NoModel, TError>(content.Kind), error));
    }

    /// <summary>Runs a pipeline when the server answers a request with one status and the request names no route for it, without reading the body.</summary>
    /// <param name="status">The status the route is for, from 300 to 599.</param>
    /// <param name="content">The kind of a route with no data.</param>
    /// <param name="error">The route's pipeline, with the reply.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults OnError(int status, NoContent content, Action<Response<NoModel>, Pipeline<NoModel>> error)
    {
        if (content is null) throw new ArgumentNullException(nameof(content));
        if (error is null) throw new ArgumentNullException(nameof(error));
        return Status(status, ErrorBody.None, routes => routes.Route(new Response<NoModel>(), error));
    }

    /// <summary>Runs a pipeline for every outcome of a request that neither it nor a default route takes.</summary>
    /// <param name="failed">The route's pipeline, with what is known of the failure.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults OnError(Action<Failure, Pipeline<NoModel>> failed)
    {
        if (failed is null) throw new ArgumentNullException(nameof(failed));
        return Otherwise(OtherwiseBody.None, routes => routes.Route(new Failure(), failed));
    }

    /// <summary>Runs a pipeline for every outcome of a request that neither it nor a default route takes, and reads the JSON body of an answer whose status is not 2xx as <typeparamref name="TError"/>.</summary>
    /// <param name="failed">The route's pipeline, with what is known of the failure.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults OnError<TError>(Action<Failure<TError>, Pipeline<NoModel>> failed)
    {
        if (failed is null) throw new ArgumentNullException(nameof(failed));
        return Otherwise(OtherwiseBody.Json, routes => routes.Route(new Failure<TError>(), failed));
    }

    /// <summary>Sets the numbers of the one retry policy: a GET, HEAD, OPTIONS, PUT or DELETE is sent again after a network failure and after a 408, 429, 502, 503 or 504.</summary>
    /// <remarks>Without this call the policy sends a request again 3 times, the first after a wait drawn between 1 and 3 seconds. A POST is never sent again, and a request opts out with <c>NoRetry()</c>.</remarks>
    /// <param name="attempts">How many times a request is sent again after its first attempt, from 1.</param>
    /// <param name="firstWait">The wait each retry's wait is drawn from, in whole milliseconds and under 30 seconds, the time a request's retries end at: the nth retry waits it times 2 to the power n - 1, times a factor drawn between one half and one and a half, and a 429's or 503's <c>Retry-After</c> takes its place.</param>
    /// <returns>The defaults.</returns>
    public RequestDefaults Retry(int attempts, TimeSpan firstWait)
    {
        if (_retrySet) throw new InvalidOperationException("AddReactiveRequests sets the retry once; call Retry(attempts, firstWait) once.");
        if (attempts < 1)
            throw new InvalidOperationException($"Retry({attempts}, ..) retries no request; give attempts from 1, and write NoRetry() on a request that is sent once.");
        if (firstWait < TimeSpan.Zero || firstWait >= RetriesEnd)
            throw new InvalidOperationException($"Retry's first wait of {firstWait} is outside 0 to 30 seconds, the time a request's retries end at; give a first wait under 30 seconds.");
        if (firstWait.Ticks % TimeSpan.TicksPerMillisecond != 0)
            throw new InvalidOperationException($"Retry's first wait of {firstWait} is not a whole number of milliseconds; give it in whole milliseconds.");
        _retrySet = true;
        Attempts = attempts;
        FirstWaitMilliseconds = (int)(firstWait.Ticks / TimeSpan.TicksPerMillisecond);
        return this;
    }

    private static void NoSteps(Pipeline<NoModel> pipeline)
    {
    }

    private RequestDefaults Status(int status, ErrorBody kind, Func<IDefaultRoutes, IReadOnlyList<Statement>> route)
    {
        if (status is < 300 or > 599)
            throw new InvalidOperationException($"OnError status {status} is outside 300 to 599; name an error status in that range.");
        var amongThem = status == ValidationErrorsStatus ? ", ShowValidationErrors() among them" : "";
        if (_statuses.Any(written => written.Status == status))
            throw new InvalidOperationException($"AddReactiveRequests already has a default route for status {status}; write one route a status{amongThem}.");
        _statuses.Add(new DefaultStatus(status, kind, route));
        return this;
    }

    private RequestDefaults Otherwise(OtherwiseBody kind, Func<IDefaultRoutes, IReadOnlyList<Statement>> route)
    {
        if (_otherwise.Count > 0) throw new InvalidOperationException("AddReactiveRequests already has a catch-all route; write one default catch-all.");
        _otherwise.Add(new DefaultOtherwise(kind, route));
        return this;
    }

    private RequestDefaults Chosen(AntiforgeryChoice choice)
    {
        if (AntiforgeryChoice != AntiforgeryChoice.NotChosen)
            throw new InvalidOperationException("AddReactiveRequests makes one antiforgery choice; call Antiforgery(AntiforgeryIn.Header), Antiforgery(AntiforgeryIn.FormField) or NoAntiforgery() once.");
        AntiforgeryChoice = choice;
        return this;
    }
}

internal interface IDefaultRoutes
{
    IReadOnlyList<Statement> Route<TReply>(TReply reply, Action<TReply, Pipeline<NoModel>> body)
        where TReply : JsObject;
}

internal sealed class DefaultStatus(int status, ErrorBody kind, Func<IDefaultRoutes, IReadOnlyList<Statement>> write)
{
    internal int Status { get; } = status;

    internal ErrorBody Kind { get; } = kind;

    internal Func<IDefaultRoutes, IReadOnlyList<Statement>> Write { get; } = write;
}

internal sealed class DefaultOtherwise(OtherwiseBody kind, Func<IDefaultRoutes, IReadOnlyList<Statement>> write)
{
    internal OtherwiseBody Kind { get; } = kind;

    internal Func<IDefaultRoutes, IReadOnlyList<Statement>> Write { get; } = write;
}
