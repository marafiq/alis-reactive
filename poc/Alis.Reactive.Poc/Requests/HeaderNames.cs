using System;
using System.Linq;

namespace Alis.Reactive.Requests;

internal static class HeaderNames
{
    private const string TokenMarks = "!#$%&'*+-.^_`|~";

    private static readonly string[] NeverHandedToAPage = ["Set-Cookie", "Set-Cookie2"];

    internal static string Checked(string name)
    {
        if (IsAToken(name)) return name;
        throw new ArgumentException($"'{name}' is no header name: a header name is letters, digits and {TokenMarks}, as X-Requested-With.", nameof(name));
    }

    internal static string Readable(string name)
    {
        Checked(name);
        if (NeverHandedToAPage.Any(each => string.Equals(name, each, StringComparison.OrdinalIgnoreCase))) throw new ArgumentException($"'{name}' is a header the browser never hands a page (the Fetch Standard's forbidden response header names), so it reads null on every answer: send what the page needs in a header of its own.", nameof(name));
        return name;
    }

    private static bool IsAToken(string name) => name.Length > 0 && name.All(character => character is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= '0' and <= '9') || TokenMarks.Contains(character));
}
