namespace Alis.Reactive.Tracing;

/// <summary>
/// The app's tracing: the activity source the framework records its server work on, and what a traced page prints.
/// </summary>
/// <remarks>
/// A page traces only when the app recorded the request that rendered it, so the app's own sampler decides which pages
/// trace. Choose the options once at startup with <c>services.AddReactiveTracing(tracing =&gt; ..)</c>.
/// </remarks>
public sealed class ReactiveTracing
{
    /// <summary>The name of the activity source the framework records a plan's render, an endpoint's check and an error answer on.</summary>
    /// <remarks>Name it in the app's OpenTelemetry setup, <c>tracing.AddSource(ReactiveTracing.SourceName)</c>, so the app records them.</remarks>
    public const string SourceName = "Alis.Reactive";

    internal ReactiveTracing()
    {
    }

    /// <summary>Gets or sets whether a traced page prints each span it ends to the browser's console. Default <c>false</c>.</summary>
    /// <remarks>Set it from the host's development setting, so a developer reads the page's spans while working and production prints none.</remarks>
    public bool Console { get; set; }
}
