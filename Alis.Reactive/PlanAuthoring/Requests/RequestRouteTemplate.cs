using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class RequestRouteTemplate
    {
        private readonly string _url;
        private readonly IReadOnlyList<string> _placeholders;

        private RequestRouteTemplate(string url, IReadOnlyList<string> placeholders)
        {
            _url = url ?? throw new ArgumentNullException(nameof(url));
            _placeholders = placeholders ?? throw new ArgumentNullException(nameof(placeholders));
        }

        internal static RequestRouteTemplate For(string url) =>
            new RequestRouteTemplate(
                url ?? throw new ArgumentNullException(nameof(url)),
                ReadPlaceholders(url));

        // A top-level request member fills the placeholder of its name, matched as MVC matches route
        // values: ignoring case. The placeholder keeps the URL's own spelling.
        internal string? PlaceholderFor(BindingPath member)
        {
            var isTopLevel = member.Value.IndexOfAny(new[] { '.', '[' }) < 0;
            if (!isTopLevel) return null;

            foreach (var placeholder in _placeholders)
            {
                if (string.Equals(placeholder, member.Value, StringComparison.OrdinalIgnoreCase))
                    return placeholder;
            }

            return null;
        }

        internal void RequireEveryPlaceholderFilled(IEnumerable<string> filled)
        {
            var filledNames = new HashSet<string>(filled, StringComparer.Ordinal);
            foreach (var placeholder in _placeholders)
            {
                if (filledNames.Contains(placeholder)) continue;

                throw new InvalidOperationException(
                    $"URL template '{_url}' has placeholder '{{{placeholder}}}', but the gathered request sets " +
                    $"no member named '{placeholder}'. Gather<TRequest>(g => g.Set(x => x.{char.ToUpperInvariant(placeholder[0])}{placeholder.Substring(1)}, ..)) fills it.");
            }
        }

        private static IReadOnlyList<string> ReadPlaceholders(string url)
        {
            var names = new List<string>();
            var text = url;
            for (var index = 0; index < text.Length; index++)
            {
                var current = text[index];
                if (current == '{')
                {
                    index = ReadPlaceholderAt(url, index, names);
                    continue;
                }

                if (current == '}')
                    throw InvalidTemplate(url, "unexpected closing brace '}'");
            }

            return names;
        }

        private static int ReadPlaceholderAt(string url, int startIndex, List<string> names)
        {
            var text = url;
            var endIndex = text.IndexOf('}', startIndex + 1);
            if (endIndex < 0)
                throw InvalidTemplate(url, "missing closing brace '}'");

            var placeholderName = text.Substring(startIndex + 1, endIndex - startIndex - 1);
            try
            {
                names.Add(RouteParameterName.Of(placeholderName).Value);
            }
            catch (ArgumentException ex)
            {
                throw InvalidTemplate(
                    url,
                    $"invalid placeholder '{{{placeholderName}}}'. Names must match [a-zA-Z0-9_] (ASCII only)",
                    ex);
            }

            return endIndex;
        }

        private static InvalidOperationException InvalidTemplate(string url, string reason) =>
            InvalidTemplate(url, reason, null);

        private static InvalidOperationException InvalidTemplate(string url, string reason, Exception? inner) =>
            new InvalidOperationException(
                $"URL template '{url}' is invalid: {reason}.",
                inner);
    }
}
