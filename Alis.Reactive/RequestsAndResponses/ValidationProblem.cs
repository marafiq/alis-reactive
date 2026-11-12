using System.Collections.Generic;

namespace Alis.Reactive.RequestsAndResponses;

/// <summary>
/// The body of an answer that carries a server's validation errors, the same on both hosts.
/// </summary>
/// <remarks>
/// A route of status 400 reads it and shows it as the browser shows its own verdict: every message in the
/// request's summary, and each also at the input the request sent under its key.
/// </remarks>
public sealed class ValidationProblem
{
    /// <summary>Gets the messages of each member that failed, keyed by the name the member binds under.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors { get; init; } = new Dictionary<string, IReadOnlyList<string>>();
}
