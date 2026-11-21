using Alis.Reactive.Conformance.World;
using OpenTelemetry.Trace;

namespace Alis.Reactive.Conformance.App.World;

internal sealed class RendersSampler(IHttpContextAccessor requests, Worlds worlds) : Sampler
{
    public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
        => new(requests.HttpContext is { } context && RendersAPage(context.Request) ? SamplingDecision.RecordAndSample : SamplingDecision.Drop);

    private bool RendersAPage(HttpRequest request) => WorldArea.IsADocuments(request) && !HoldsAnUnrecordedRender(request);

    private bool HoldsAnUnrecordedRender(HttpRequest request)
        => request.Cookies.TryGetValue(WorldKey.Cookie, out var key) && !string.IsNullOrEmpty(key) && worlds.Of(key).RenderUnrecorded;
}
