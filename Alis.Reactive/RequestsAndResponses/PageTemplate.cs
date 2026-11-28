using System;
using System.Collections.Generic;
using System.Linq;

namespace Alis.Reactive.RequestsAndResponses;

internal sealed class PageTemplate
{
    private const char Apart = '/';
    private const char Open = '{';
    private const char Close = '}';
    private const char Constrained = ':';
    private const string FromTheAppsRoot = "~/";

    private static readonly char[] PastThePath = ['?', '#'];
    private static readonly char[] PastAConstraint = ['?', '=', Open, Close];

    internal static readonly PageTemplate None = new([], new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));

    private readonly IReadOnlyDictionary<string, int> _placeholders;

    private PageTemplate(IReadOnlyList<string> segments, IReadOnlyDictionary<string, int> placeholders)
    {
        Segments = segments;
        _placeholders = placeholders;
    }

    internal IReadOnlyList<string> Segments { get; }

    internal bool Holds(string name, out int place) => _placeholders.TryGetValue(name, out place);

    internal static PageTemplate Of(string template, string pathBase, IReadOnlyCollection<string> members, string page)
    {
        if (template is null) throw new ArgumentNullException(nameof(template));
        var segments = pathBase.Split(Apart).Where(segment => segment.Length > 0).ToList();
        var placeholders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var path = PathOf(template);
        if (path.Length == 0) return new PageTemplate(segments, placeholders);
        foreach (var segment in path.Split(Apart))
        {
            if (segment.Length == 0) throw HoldsAnEmptySegment(template);
            if (segment.IndexOf(Open) < 0 && segment.IndexOf(Close) < 0)
            {
                if (segment.IndexOfAny(PastThePath) >= 0) throw NamesMoreThanAPath(template);
                segments.Add(segment);
                continue;
            }

            if (!IsAWholePlaceholder(segment, out var name)) throw HoldsAPlaceholderItCannotRead(template, segment);
            if (!members.Contains(name, StringComparer.OrdinalIgnoreCase)) throw NamesNoMember(template, name, page);
            if (placeholders.ContainsKey(name)) throw NamesAPlaceholderTwice(template, name);
            placeholders.Add(name, segments.Count);
            segments.Add(Open + name + Close);
        }

        return new PageTemplate(segments, placeholders);
    }

    private static string PathOf(string template)
    {
        var fromTheRoot = template.StartsWith(FromTheAppsRoot, StringComparison.Ordinal) ? template.Substring(FromTheAppsRoot.Length)
            : template.Length > 0 && template[0] == Apart ? template.Substring(1) : template;
        return fromTheRoot.Length > 0 && fromTheRoot[fromTheRoot.Length - 1] == Apart ? fromTheRoot.Substring(0, fromTheRoot.Length - 1) : fromTheRoot;
    }

    private static bool IsAWholePlaceholder(string segment, out string name)
    {
        name = "";
        if (segment.Length < 3 || segment[0] != Open || segment[segment.Length - 1] != Close) return false;
        var inside = segment.Substring(1, segment.Length - 2);
        var constraintAt = inside.IndexOf(Constrained);
        var named = constraintAt < 0 ? inside : inside.Substring(0, constraintAt);
        var constraint = constraintAt < 0 ? "" : inside.Substring(constraintAt + 1);
        if (named.Length == 0 || !named.All(character => char.IsLetterOrDigit(character) || character == '_')) return false;
        if (constraintAt >= 0 && (constraint.Length == 0 || constraint.IndexOfAny(PastAConstraint) >= 0)) return false;
        name = named;
        return true;
    }

    private static ArgumentException HoldsAnEmptySegment(string template)
        => new($"The page's URL template '{template}' holds an empty segment: write one '/' between two segments.", nameof(template));

    private static ArgumentException NamesMoreThanAPath(string template)
        => new($"The page's URL template '{template}' holds a query or a fragment: a template names the page's path alone.", nameof(template));

    private static ArgumentException HoldsAPlaceholderItCannotRead(string template, string segment)
        => new($"The page's URL template '{template}' holds '{segment}': the page's URL reads a placeholder that is a whole segment, {{Name}} or {{Name:constraint}}, with no default value, optional mark or catch-all.", nameof(template));

    private static ArgumentException NamesNoMember(string template, string name, string page)
        => new($"The page's URL template '{template}' names {{{name}}}, which {page} does not declare: name each placeholder after the member of {page} its path holds.", nameof(template));

    private static ArgumentException NamesAPlaceholderTwice(string template, string name)
        => new($"The page's URL template '{template}' names {{{name}}} twice.", nameof(template));
}
