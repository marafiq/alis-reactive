using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// Where a <see cref="FusionSmartTextArea"/> shows a suggestion, written by EJ2's name.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<FusionSmartSuggestionMode>))]
public enum FusionSmartSuggestionMode
{
    /// <summary>Over the text on a touch device, and inline in the text elsewhere.</summary>
    [JsonStringEnumMemberName("None")]
    None,

    /// <summary>Inline in the text, taken with the Tab key.</summary>
    [JsonStringEnumMemberName("Disable")]
    Inline,

    /// <summary>Over the text, taken with a tap or a click.</summary>
    [JsonStringEnumMemberName("Enable")]
    Popup,
}
