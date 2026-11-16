using System.Diagnostics;
using System.Globalization;

namespace Alis.Reactive.Conformance.App.World;

internal sealed record CollectedSpan(string TraceId, string SpanId, string ParentId, string Scope, string Name, long StartUnixNanos, string? Outcome, int? Status, int? Resend)
{
    private const string OutcomeKey = "alis.outcome";
    private const string StatusKey = "http.response.status_code";
    private const string ResendKey = "http.request.resend_count";
    private const string NoParent = "";
    private const long NanosPerTick = 100;

    internal string Facts => string.Join(' ', FactsHeld());

    internal static CollectedSpan Of(OtlpSpan span, string scope)
        => new(span.TraceId, span.SpanId, span.ParentSpanId ?? NoParent, scope, span.Name, long.Parse(span.StartTimeUnixNano, CultureInfo.InvariantCulture),
            TextOf(span, OutcomeKey), NumberOf(span, StatusKey), NumberOf(span, ResendKey));

    internal static CollectedSpan Of(Activity activity)
        => new(activity.TraceId.ToHexString(), activity.SpanId.ToHexString(), activity.ParentSpanId.ToHexString(), activity.Source.Name, activity.OperationName,
            (activity.StartTimeUtc - DateTime.UnixEpoch).Ticks * NanosPerTick, null, null, null);

    private IEnumerable<string> FactsHeld()
    {
        if (Outcome is { } outcome) yield return outcome;
        if (Status is { } status) yield return status.ToString(CultureInfo.InvariantCulture);
        if (Resend is { } resend) yield return "resend " + resend.ToString(CultureInfo.InvariantCulture);
    }

    private static string? TextOf(OtlpSpan span, string key) => span.Attributes.FirstOrDefault(attribute => attribute.Key == key)?.Value.StringValue;

    private static int? NumberOf(OtlpSpan span, string key) => span.Attributes.FirstOrDefault(attribute => attribute.Key == key)?.Value.IntValue is { } number ? checked((int)number) : null;
}
