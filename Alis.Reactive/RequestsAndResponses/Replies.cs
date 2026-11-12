using System;
using Alis.Reactive.Declarations;

namespace Alis.Reactive.RequestsAndResponses;

/// <summary>
/// The reply a request's route reads: its status and its headers, each a source in the route and in the blocks inside it.
/// </summary>
/// <typeparam name="TRequest">The request's type, whose members the reply reads back as they were sent.</typeparam>
public class Response<TRequest> : JsObject
{
    internal Response()
    {
    }

    /// <summary>Gets the status of the server's answer.</summary>
    public ReadOnly<int> Status => throw new NotImplementedException();

    /// <summary>Gets the headers of the server's answer, as the browser hands them to the page.</summary>
    public ResponseHeaders Headers => throw new NotImplementedException();
}

/// <summary>
/// The reply a request's route reads, with its body read as the kind the route declares.
/// </summary>
/// <typeparam name="TRequest">The request's type, whose members the reply reads back as they were sent.</typeparam>
/// <typeparam name="TBody">The type the route reads the body as.</typeparam>
public sealed class Response<TRequest, TBody> : Response<TRequest>
{
    internal Response()
    {
    }

    /// <summary>Gets the body of the server's answer; <c>Read</c> reads a field of it.</summary>
    public ReadOnly<TBody> Body => throw new NotImplementedException();
}

/// <summary>
/// The headers of a server's answer.
/// </summary>
public sealed class ResponseHeaders : JsObject
{
    internal ResponseHeaders()
    {
    }

    /// <summary>Reads a header by its name.</summary>
    /// <param name="name">The header's name, compared ignoring case.</param>
    /// <returns>A source of the header's text, which cannot be read when the answer lacks the header.</returns>
    public MethodResult<string?> Get(string name) => throw new NotImplementedException();
}

/// <summary>
/// The body of an answer read as HTML, which <c>Into</c> loads into an element.
/// </summary>
public sealed class Markup
{
    private Markup()
    {
    }
}

/// <summary>
/// Why a request ran its catch-all route: one of four outcomes that are not a success.
/// </summary>
public sealed class FailureOutcome
{
    /// <summary>The server answered with a status that is not 2xx, and the request names no route for that status.</summary>
    public static readonly FailureOutcome Status = new();

    /// <summary>The network failed, or the answer's body broke off while it was read.</summary>
    public static readonly FailureOutcome Network = new();

    /// <summary>A redirect ended on another origin than the page's.</summary>
    public static readonly FailureOutcome Redirect = new();

    /// <summary>The server answered 2xx with a body that is not of the kind the success route declares.</summary>
    public static readonly FailureOutcome Content = new();

    private FailureOutcome()
    {
    }
}

/// <summary>
/// What a request's catch-all route reads of a request that did not succeed.
/// </summary>
public sealed class Failure : JsObject
{
    internal Failure()
    {
    }

    /// <summary>Gets why the request did not succeed.</summary>
    public ReadOnly<FailureOutcome> Outcome => throw new NotImplementedException();

    /// <summary>Gets the status of the server's answer.</summary>
    public ReadOnly<int> Status => throw new NotImplementedException();

    /// <summary>Gets the headers of the server's answer, as the browser hands them to the page.</summary>
    public ResponseHeaders Headers => throw new NotImplementedException();
}

/// <summary>
/// What a request's catch-all route reads of a request that did not succeed, with the body of an answer whose
/// status is not 2xx.
/// </summary>
/// <typeparam name="TError">The type the route reads the body as.</typeparam>
public sealed class Failure<TError> : JsObject
{
    internal Failure()
    {
    }

    /// <summary>Gets why the request did not succeed.</summary>
    public ReadOnly<FailureOutcome> Outcome => throw new NotImplementedException();

    /// <summary>Gets the status of the server's answer.</summary>
    public ReadOnly<int> Status => throw new NotImplementedException();

    /// <summary>Gets the headers of the server's answer, as the browser hands them to the page.</summary>
    public ResponseHeaders Headers => throw new NotImplementedException();

    /// <summary>Gets the body of an answer whose status is not 2xx; <c>Read</c> reads a field of it.</summary>
    public ReadOnly<TError> Body => throw new NotImplementedException();
}
