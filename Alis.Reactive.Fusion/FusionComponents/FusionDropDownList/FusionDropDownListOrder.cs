using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The order of the choices in a drop-down list.</summary>
[JsonConverter(typeof(FusionDropDownListStringEnumConverter<FusionDropDownListOrder>))]
public enum FusionDropDownListOrder
{
    /// <summary>Preserves the order of the source.</summary>
    None,

    /// <summary>Sorts in ascending order.</summary>
    Ascending,

    /// <summary>Sorts in descending order.</summary>
    Descending,
}
