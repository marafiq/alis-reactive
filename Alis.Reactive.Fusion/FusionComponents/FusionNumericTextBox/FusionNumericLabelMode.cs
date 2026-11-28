using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The three floating-caption modes supported by the numeric editor.</summary>
[JsonConverter(typeof(FusionNumericLabelModeConverter))]
public enum FusionNumericLabelMode
{
    /// <summary>Show the caption as an inline hint.</summary>
    [JsonStringEnumMemberName("Never")]
    Never,

    /// <summary>Keep the caption above the field.</summary>
    [JsonStringEnumMemberName("Always")]
    Always,

    /// <summary>Raise the caption on focus or while the field holds a number.</summary>
    [JsonStringEnumMemberName("Auto")]
    Auto,
}

internal sealed class FusionNumericLabelModeConverter : JsonStringEnumConverter<FusionNumericLabelMode>
{
    public FusionNumericLabelModeConverter()
        : base(namingPolicy: null, allowIntegerValues: false)
    {
    }
}
