using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The culture-specific date pattern identified by its skeleton.</summary>
public sealed class FusionDateFormatPattern
{
    /// <summary>Gets or sets the date skeleton used by the configured culture.</summary>
    [JsonPropertyName("skeleton")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Skeleton { get; set; }
}
