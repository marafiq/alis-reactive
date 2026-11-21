using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Requests;

namespace Alis.Reactive.Pipelines;

internal static class RequestHeaders
{
    private static readonly string[] SetByTheBrowser =
    [
        "Accept-Charset", "Accept-Encoding", "Access-Control-Request-Headers", "Access-Control-Request-Method", "Connection",
        "Content-Length", "Cookie", "Cookie2", "Date", "DNT", "Expect", "Host", "Keep-Alive", "Origin", "Referer", "Set-Cookie",
        "TE", "Trailer", "Transfer-Encoding", "Upgrade", "Via",
    ];

    private static readonly string[] TraceContext = ["traceparent", "tracestate", "baggage"];

    private static readonly string[] KeptByTheBrowser = ["Proxy-", "Sec-"];

    private static readonly string[] MethodOverrides = ["X-HTTP-Method", "X-HTTP-Method-Override", "X-Method-Override"];

    internal static void RequireANewName(string name, IReadOnlyList<SentMember> sent)
    {
        HeaderNames.Checked(name);
        if (Is(name, "Content-Type")) throw new ArgumentException("Content-Type is set by the body door, and a request with no body sends none.", nameof(name));
        if (Is(name, "Accept")) throw new ArgumentException("Accept is set by the kind the success route reads, never by a header the view writes.", nameof(name));
        if (Is(name, "RequestVerificationToken")) throw new ArgumentException("RequestVerificationToken carries the antiforgery token, which the framework sends, never a header the view writes.", nameof(name));
        if (TraceContext.Any(each => Is(name, each))) throw new ArgumentException($"{name} carries the trace context the framework sends, never a header the view writes.", nameof(name));
        if (SetByTheBrowser.Any(each => Is(name, each))) throw new ArgumentException($"{name} is a header the browser sets itself and never sends from a page (the Fetch Standard's forbidden request headers).", nameof(name));
        if (KeptByTheBrowser.FirstOrDefault(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) is { } kept)
        {
            throw new ArgumentException($"{name} starts with {kept}, which the browser keeps for the headers it sets itself (the Fetch Standard's forbidden request headers).", nameof(name));
        }

        if (MethodOverrides.Any(each => Is(name, each))) throw new ArgumentException($"{name} overrides the request's method, which its verb states; write the verb.", nameof(name));
        if (sent.Any(each => Is(each.Name, name))) throw new ArgumentException($"The request sends the header {name} twice; send each header once.", nameof(name));
    }

    internal static string FixedValue(string name, string value)
    {
        var trimmed = value.Trim(' ', '\t');
        if (trimmed.IndexOfAny(['\r', '\n']) >= 0) throw new ArgumentException($"The header {name} holds a line break, which HTTP does not carry in a field value; send it in the query.", nameof(value));
        var outside = trimmed.FirstOrDefault(character => !CarriedInAValue(character));
        if (outside != default) throw new ArgumentException($"The header {name} holds '{outside}', which a header value does not carry: a header carries visible US-ASCII, spaces and tabs; send it in the query.", nameof(value));
        return trimmed;
    }

    private static bool CarriedInAValue(char character) => character is '\t' or (>= ' ' and <= '~');

    private static bool Is(string name, string other) => string.Equals(name, other, StringComparison.OrdinalIgnoreCase);
}
