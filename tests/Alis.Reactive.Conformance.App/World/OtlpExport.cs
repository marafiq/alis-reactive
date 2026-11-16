namespace Alis.Reactive.Conformance.App.World;

internal sealed record OtlpExport(IList<OtlpResourceSpans> ResourceSpans);

internal sealed record OtlpResourceSpans(IList<OtlpScopeSpans> ScopeSpans);

internal sealed record OtlpScopeSpans(OtlpScope Scope, IList<OtlpSpan> Spans);

internal sealed record OtlpScope(string Name);

internal sealed record OtlpSpan(string TraceId, string SpanId, string? ParentSpanId, string Name, string StartTimeUnixNano, IList<OtlpKeyValue> Attributes);

internal sealed record OtlpKeyValue(string Key, OtlpAnyValue Value);

internal sealed record OtlpAnyValue(string? StringValue, long? IntValue);
