namespace Alis.Reactive.Fusion.Components
{
    /// <summary>Event payload delivered after a failed commit of a <see cref="FusionInPlaceEditor"/>.</summary>
    /// <remarks>
    /// Only meaningful when SF's UrlAdaptor is configured with a URL that can fail.
    /// With no url configured, SF's internal submit is a no-op and this event never fires.
    /// </remarks>
    public class FusionInPlaceEditorActionFailureArgs
    {
        /// <summary>Error response data from SF's adaptor.</summary>
        public object? Data { get; set; }

        /// <summary>The value that was attempted.</summary>
        public string? Value { get; set; }

        /// <summary>The SF event name.</summary>
        public string? Name { get; set; }

        /// <summary>Creates a new instance. Framework-internal: instances are created by the event descriptor.</summary>
        public FusionInPlaceEditorActionFailureArgs() { }
    }
}
