using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class RequestRouteTemplate
    {
        private readonly RequestUrl _url;
        private readonly IReadOnlyList<string> _placeholders;
        private readonly HashSet<string> _placeholderLookup;

        private RequestRouteTemplate(RequestUrl url, IReadOnlyList<string> placeholders)
        {
            _url = url ?? throw new ArgumentNullException(nameof(url));
            _placeholders = placeholders ?? throw new ArgumentNullException(nameof(placeholders));
            _placeholderLookup = new HashSet<string>(_placeholders, StringComparer.Ordinal);
        }

        internal static RequestRouteTemplate For(RequestUrl url) =>
            new RequestRouteTemplate(
                url ?? throw new ArgumentNullException(nameof(url)),
                ReadPlaceholders(url));

        internal void RequireRouteParameters(IEnumerable<string> routeParameterNames)
        {
            if (routeParameterNames == null) throw new ArgumentNullException(nameof(routeParameterNames));

            var suppliedNames = new HashSet<string>(routeParameterNames, StringComparer.Ordinal);
            EnsureEveryRouteParameterHasPlaceholder(suppliedNames);
            EnsureEveryPlaceholderHasRouteParameter(suppliedNames);
        }

        private void EnsureEveryRouteParameterHasPlaceholder(ISet<string> routeParameterNames)
        {
            foreach (var paramName in routeParameterNames)
            {
                if (_placeholderLookup.Contains(paramName)) continue;

                throw new InvalidOperationException(
                    $"Route param '{paramName}' does not match any placeholder in URL '{_url.Value}'. " +
                    $"Expected '{{{paramName}}}' in the URL template.");
            }
        }

        private void EnsureEveryPlaceholderHasRouteParameter(ISet<string> routeParameterNames)
        {
            foreach (var placeholder in _placeholders)
            {
                if (routeParameterNames.Contains(placeholder)) continue;

                throw new InvalidOperationException(
                    $"URL template '{_url.Value}' has placeholder '{{{placeholder}}}' " +
                    $"but no matching .RouteParam(\"{placeholder}\", ...) was provided.");
            }
        }

        private static IReadOnlyList<string> ReadPlaceholders(RequestUrl url)
        {
            var names = new List<string>();
            var text = url.Value;
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

        private static int ReadPlaceholderAt(RequestUrl url, int startIndex, List<string> names)
        {
            var text = url.Value;
            var endIndex = text.IndexOf('}', startIndex + 1);
            if (endIndex < 0)
                throw InvalidTemplate(url, "missing closing brace '}'");

            var name = text.Substring(startIndex + 1, endIndex - startIndex - 1);
            try
            {
                names.Add(RouteParameterName.Of(name).Value);
            }
            catch (ArgumentException ex)
            {
                throw InvalidTemplate(
                    url,
                    $"invalid placeholder '{{{name}}}'. Names must match [a-zA-Z0-9_] (ASCII only)",
                    ex);
            }

            return endIndex;
        }

        private static InvalidOperationException InvalidTemplate(RequestUrl url, string reason) =>
            InvalidTemplate(url, reason, null);

        private static InvalidOperationException InvalidTemplate(RequestUrl url, string reason, Exception? inner) =>
            new InvalidOperationException(
                $"URL template '{url.Value}' is invalid: {reason}.",
                inner);
    }
}
