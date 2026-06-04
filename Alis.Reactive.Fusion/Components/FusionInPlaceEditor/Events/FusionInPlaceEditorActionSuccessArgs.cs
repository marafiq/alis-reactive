namespace Alis.Reactive.Fusion.Components
{
    /// <summary>Event payload delivered after a successful commit of a <see cref="FusionInPlaceEditor"/>.</summary>
    /// <remarks>
    /// Primary hook for the reactive commit pipeline. Fires only after validation passes. Editor is still
    /// open and display text is still stale when this fires; Syncfusion closes the editor immediately after.
    /// </remarks>
    public class FusionInPlaceEditorActionSuccessArgs
    {
        /// <summary>Server response data when Syncfusion's UrlAdaptor is configured; empty <c>{}</c> when no url.</summary>
        public object? Data { get; set; }

        /// <summary>The value that was committed.</summary>
        public string? Value { get; set; }

        /// <summary>The Syncfusion event name.</summary>
        public string? Name { get; set; }
    }
}
