using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The ways a list can match its search text.</summary>
[JsonConverter(typeof(FusionDropDownListStringEnumConverter<FusionDropDownListMatchMode>))]
public enum FusionDropDownListMatchMode
{
    /// <summary>Matches the start of the label.</summary>
    StartsWith,

    /// <summary>Matches the end of the label.</summary>
    EndsWith,

    /// <summary>Matches text anywhere in the label.</summary>
    Contains,
}
