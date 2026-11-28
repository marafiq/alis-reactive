using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The presentation of the list’s floating label.</summary>
[JsonConverter(typeof(FusionDropDownListStringEnumConverter<FusionDropDownListLabelMode>))]
public enum FusionDropDownListLabelMode
{
    /// <summary>Shows the native placeholder.</summary>
    Never,

    /// <summary>Keeps the label above the field.</summary>
    Always,

    /// <summary>Floats the label when the field is focused or filled.</summary>
    Auto,
}
