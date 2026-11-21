using System;
using System.Collections.Generic;

namespace Alis.Reactive.RequestsAndResponses;

internal static class HeaderNames
{
    private static readonly HashSet<string> Owned = new(StringComparer.OrdinalIgnoreCase)
    {
        "RequestVerificationToken", "traceparent", "tracestate", "baggage",
        "Accept-Charset", "Accept-Encoding", "Access-Control-Request-Headers", "Access-Control-Request-Method",
        "Connection", "Content-Length", "Cookie", "Cookie2", "Date", "DNT", "Expect", "Host", "Keep-Alive",
        "Origin", "Permissions-Policy", "Referer", "Set-Cookie", "TE", "Trailer", "Transfer-Encoding", "Upgrade",
        "Via", "X-HTTP-Method", "X-HTTP-Method-Override", "X-Method-Override",
    };

    internal static void Request(string name)
    {
        Token(name);
        if (name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Content-Type is set by the body door; write Json, UrlEncoded, Multipart, Text or File.");
        if (name.Equals("Accept", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Accept is set by the success route; choose its Content kind.");
        if (Owned.Contains(name) || name.StartsWith("Proxy-", StringComparison.OrdinalIgnoreCase) || name.StartsWith("Sec-", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Header '{name}' is written by the framework or the browser; use an app header name.");
    }

    internal static string Response(string name)
    {
        Token(name);
        if (name.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase) || name.Equals("Set-Cookie2", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Response header '{name}' is withheld by the browser; read an exposed response header.");
        return name;
    }

    internal static void Value(string name, string value)
    {
        foreach (var character in value)
        {
            if (character is '\r' or '\n')
                throw new InvalidOperationException($"The header {name} holds a line break, which HTTP does not carry in a field value; send it in the body.");
            if (character != '\t' && (character < ' ' || character > '~'))
                throw new InvalidOperationException($"Header '{name}' holds a character outside visible US-ASCII, spaces and tabs; send that text in the body.");
        }
    }

    private static void Token(string name)
    {
        var valid = name.Length > 0;
        foreach (var character in name)
            valid &= character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' || "!#$%&'*+-.^_`|~".IndexOf(character) >= 0;
        if (!valid)
            throw new InvalidOperationException($"Header '{name}' is not an HTTP token; use a name made of HTTP token characters.");
    }
}
