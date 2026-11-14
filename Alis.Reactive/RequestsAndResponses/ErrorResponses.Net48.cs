#if NET48
using System;
using System.Web.Mvc;
using Alis.Reactive.Validation;

namespace Alis.Reactive.RequestsAndResponses;

/// <summary>
/// The answer an endpoint gives when the server's check of what a request sent fails.
/// </summary>
public static class ErrorResponses
{
    /// <summary>Answers a failed check with status 400 and its messages by the name each input was sent under.</summary>
    /// <remarks>
    /// The body is <c>{"errors": {"Room.Number": ["Room is full"]}}</c>, the one body both hosts send, so the page shows
    /// each message beside the input it names and every message in the request's summary. IIS keeps the body rather
    /// than its own error page.
    /// </remarks>
    /// <param name="result">The check's result.</param>
    /// <returns>The action's answer.</returns>
    public static ActionResult ToErrorResponse(this ValidationResult result) => throw new NotImplementedException();
}
#endif
