namespace Alis.Reactive.Fusion
{
    /// <summary>The settings a view gives <see cref="FusionSmartPasteButtonHtmlExtensions.FusionSmartPasteButton{TModel}"/>.</summary>
    public sealed class FusionSmartPasteButtonOptions
    {
        /// <summary>Gets or sets the button's text.</summary>
        public string Content { get; set; } = "Smart Paste";

        /// <summary>
        /// Gets or sets the URL the button posts its request settings to; the response text is the answer it
        /// fills the form from. Empty leaves the button with no assist handler.
        /// </summary>
        public string AiAssistEndpoint { get; set; } = "";

        /// <summary>Gets or sets the CSS classes on the button element.</summary>
        public string CssClass { get; set; } = "";

        /// <summary>Gets or sets whether the button has the primary look.</summary>
        public bool IsPrimary { get; set; }
    }
}
