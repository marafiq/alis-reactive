using System.Collections.Generic;

namespace Alis.Reactive.Fusion.Components
{
    public sealed class FusionSmartTextAreaOptions
    {
        public string Value { get; set; } = "";
        public string UserRole { get; set; } = "";
        public IReadOnlyList<string> UserPhrases { get; set; } = System.Array.Empty<string>();
        public string SuggestionEndpoint { get; set; } = "";
        public FusionSmartSuggestionMode SuggestionMode { get; set; } = FusionSmartSuggestionMode.None;
        public string CssClass { get; set; } = "";
        public int Rows { get; set; } = 4;
    }

    public enum FusionSmartSuggestionMode
    {
        None,
        Inline,
        Popup
    }
}
