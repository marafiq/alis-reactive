using System.Diagnostics;
using System.Globalization;
using Alis.Reactive.Tracing;

namespace Alis.Reactive.Conformance.App.World;

internal sealed record CollectedAttribute(string Key, string Value)
{
    internal string Line => Key + "=" + Value;

    internal bool Holds(string text) => Key.Contains(text, StringComparison.OrdinalIgnoreCase) || Value.Contains(text, StringComparison.OrdinalIgnoreCase);
}

internal sealed record CollectedEvent(string Name, IReadOnlyList<CollectedAttribute> Attributes)
{
    internal string Line => string.Join(' ', ["event " + Name, .. Attributes.Select(attribute => attribute.Line)]);

    internal bool Holds(string text) => Name.Contains(text, StringComparison.OrdinalIgnoreCase) || Attributes.Any(attribute => attribute.Holds(text));
}

internal sealed record CollectedSpan(string TraceId, string SpanId, string ParentId, string Scope, string Name, long StartUnixNanos,
    IReadOnlyList<CollectedAttribute> Attributes, IReadOnlyList<CollectedEvent> Events)
{
    private const string NoParent = "";
    private const long NanosPerTick = 100;

    internal string Facts => string.Join(' ', [.. FactsHeld(), .. Events.Select(happened => happened.Line)]);

    internal bool Holds(string text)
        => Name.Contains(text, StringComparison.OrdinalIgnoreCase) || Attributes.Any(attribute => attribute.Holds(text)) || Events.Any(happened => happened.Holds(text));

    internal static CollectedSpan Of(OtlpSpan span, string scope)
        => new(span.TraceId, span.SpanId, span.ParentSpanId ?? NoParent, scope, span.Name, long.Parse(span.StartTimeUnixNano, CultureInfo.InvariantCulture),
            [.. span.Attributes.Select(AttributeOf)], [.. span.Events.Select(happened => new CollectedEvent(happened.Name, [.. happened.Attributes.Select(AttributeOf)]))]);

    internal static CollectedSpan Of(Activity activity)
        => new(activity.TraceId.ToHexString(), activity.SpanId.ToHexString(), ListedParentOf(activity), activity.Source.Name, activity.OperationName,
            (activity.StartTimeUtc - DateTime.UnixEpoch).Ticks * NanosPerTick,
            [.. activity.TagObjects.Select(tag => new CollectedAttribute(tag.Key, Convert.ToString(tag.Value, CultureInfo.InvariantCulture) ?? ""))], []);

    private IEnumerable<string> FactsHeld()
        => [.. Labeled("alis.outcome", ""), .. Labeled("http.response.status_code", ""), .. Labeled("http.request.resend_count", "resend "),
            .. Labeled("alis.validator", "validator "), .. Labeled("alis.rule_set", "rule set "), .. Labeled("alis.failure_count", "failure count ")];

    private IEnumerable<string> Labeled(string key, string label) => Attributes.Where(attribute => attribute.Key == key).Select(attribute => label + attribute.Value);

    private static string ListedParentOf(Activity activity)
    {
        if (activity.Parent is not { } parent) return activity.ParentSpanId.ToHexString();
        if (parent.Source.Name == ReactiveTracing.SourceName) return parent.SpanId.ToHexString();
        return ListedParentOf(parent);
    }

    private static CollectedAttribute AttributeOf(OtlpKeyValue attribute)
        => new(attribute.Key, attribute.Value.StringValue ?? attribute.Value.IntValue?.ToString(CultureInfo.InvariantCulture)
            ?? attribute.Value.BoolValue?.ToString(CultureInfo.InvariantCulture) ?? attribute.Value.DoubleValue?.ToString(CultureInfo.InvariantCulture) ?? "");
}
