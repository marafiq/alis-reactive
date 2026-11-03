using System.Collections.Generic;

namespace Alis.Reactive.Fusion
{
    /// <summary>The settings a view gives <see cref="FusionSmartTextAreaHtmlExtensions.FusionSmartTextArea{TModel, TProp}(InputBoundField{TModel, TProp}, System.Action{FusionSmartTextAreaOptions})"/>.</summary>
    public sealed class FusionSmartTextAreaOptions
    {
        /// <summary>Gets or sets the text the area starts with.</summary>
        public string Value { get; set; } = "";

        /// <summary>Gets or sets who is typing and why, which the suggestions are written for.</summary>
        public string UserRole { get; set; } = "";

        /// <summary>Gets or sets phrases the user often writes, which the suggestions draw on.</summary>
        public IReadOnlyList<string> UserPhrases { get; set; } = System.Array.Empty<string>();

        /// <summary>
        /// Gets or sets the URL the area posts its request settings to; the response text is the suggestion.
        /// Empty leaves the area with no suggestion handler.
        /// </summary>
        public string SuggestionEndpoint { get; set; } = "";

        /// <summary>Gets or sets where a suggestion shows.</summary>
        public FusionSmartSuggestionMode SuggestionMode { get; set; } = FusionSmartSuggestionMode.None;

        /// <summary>Gets or sets the CSS classes on the text area element.</summary>
        public string CssClass { get; set; } = "";

        /// <summary>Gets or sets the number of visible text lines.</summary>
        public int Rows { get; set; } = 4;
    }

    /// <summary>Where a <see cref="FusionSmartTextArea"/> shows a suggestion.</summary>
    public enum FusionSmartSuggestionMode
    {
        /// <summary>As an overlay on touch devices and inline elsewhere.</summary>
        None,

        /// <summary>Inline in the text, taken with the Tab key.</summary>
        Inline,

        /// <summary>In a popup the user taps or clicks.</summary>
        Popup
    }
}
