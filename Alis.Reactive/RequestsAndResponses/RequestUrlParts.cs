using System;
using System.Collections.Generic;
using System.Text;

namespace Alis.Reactive.RequestsAndResponses;

internal sealed class RequestUrlParts
{
    private readonly List<Part> _parts = [];
    private readonly string _origin = "";

    internal RequestUrlParts(string url)
    {
        Url = url;
        var queryAt = url.IndexOf('?');
        var path = queryAt < 0 ? url : url.Substring(0, queryAt);
        Query = queryAt < 0 ? "" : url.Substring(queryAt + 1);
        if (url.Length == 0 || url.IndexOf('#') >= 0 || url.IndexOf('\\') >= 0 || url.StartsWith("//", StringComparison.Ordinal) ||
            Query.IndexOfAny(['{', '}']) >= 0)
        {
            throw Invalid();
        }

        foreach (var character in url)
            if (character < '!' || character > '~') throw Invalid();
        var checkedPath = new StringBuilder();
        var position = 0;
        while (position < path.Length)
        {
            var open = path.IndexOf('{', position);
            if (open < 0)
            {
                var rest = path.Substring(position);
                if (rest.IndexOf('}') >= 0) throw Invalid();
                _parts.Add(new Part(rest, ""));
                checkedPath.Append(rest);
                break;
            }

            var text = path.Substring(position, open - position);
            var close = path.IndexOf('}', open + 1);
            if (text.IndexOf('}') >= 0 || close < 0) throw Invalid();
            var name = path.Substring(open + 1, close - open - 1);
            if (name.Length == 0) throw Invalid();
            foreach (var character in name)
                if (!(character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_')) throw Invalid();
            if (text.Length > 0) _parts.Add(new Part(text, ""));
            _parts.Add(new Part("", name));
            checkedPath.Append(text).Append("value");
            position = close + 1;
        }

        var checkedUrl = checkedPath + (queryAt < 0 ? "" : "?" + Query);
        if (!Uri.IsWellFormedUriString(checkedUrl, UriKind.RelativeOrAbsolute)) throw Invalid();
        if (!Uri.IsWellFormedUriString(checkedUrl, UriKind.Relative))
        {
            if (!Uri.TryCreate(checkedUrl, UriKind.Absolute, out var absolute)) throw Invalid();
            if (absolute.Scheme != Uri.UriSchemeHttp && absolute.Scheme != Uri.UriSchemeHttps) throw Invalid();
            var authorityEnds = path.IndexOf('/', path.IndexOf("://", StringComparison.Ordinal) + 3);
            var firstPlaceholder = path.IndexOf('{');
            if (firstPlaceholder >= 0 && (authorityEnds < 0 || firstPlaceholder < authorityEnds)) throw Invalid();
            _origin = absolute.GetLeftPart(UriPartial.Authority);
        }
    }

    internal string Url { get; }
    internal string Query { get; }
    internal IReadOnlyList<Part> Parts => _parts;

    internal bool OnOrigin(string pageOrigin)
        => _origin.Length == 0 || (Uri.TryCreate(pageOrigin, UriKind.Absolute, out var page) && string.Equals(page.GetLeftPart(UriPartial.Authority), _origin, StringComparison.OrdinalIgnoreCase));

    internal bool Has(string name)
    {
        foreach (var part in _parts)
            if (part.Member.Equals(name, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    private InvalidOperationException Invalid()
        => new($"The URL '{Url}' is not a page path or an absolute http or https URI with paired path placeholders and no fragment; write a URL of that form.");

    internal sealed record Part(string Text, string Member);
}
