namespace Alis.Reactive.Conformance.App.Care;

public sealed class TracedLayoutSink
{
    public static readonly TracedLayoutSink TheOpenTelemetryAdapter = new();

    public static readonly TracedLayoutSink TheAppsThrowingObject = new();

    private TracedLayoutSink()
    {
    }
}
