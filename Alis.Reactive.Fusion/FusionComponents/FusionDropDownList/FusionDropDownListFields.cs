using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The display, value and optional grouping fields of a drop-down list row.</summary>
/// <typeparam name="TItem">The type of the catalogue row.</typeparam>
public sealed class FusionDropDownListFields<TItem>
{
    internal FusionDropDownListFields(string text, string value, string? groupBy)
    {
        Text = text;
        Value = value;
        GroupBy = groupBy;
    }

    /// <summary>Gets the JSON name of the field that supplies the option label.</summary>
    [JsonPropertyName("text")]
    public string Text { get; }

    /// <summary>Gets the JSON name of the field that supplies the selection value.</summary>
    [JsonPropertyName("value")]
    public string Value { get; }

    /// <summary>Gets the JSON name of the grouping field, or null for an ungrouped list.</summary>
    [JsonPropertyName("groupBy")]
    public string? GroupBy { get; }
}
