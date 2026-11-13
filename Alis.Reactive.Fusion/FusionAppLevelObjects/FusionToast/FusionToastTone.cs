using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionAppLevelObjects;

/// <summary>
/// The tone of a <see cref="FusionToast"/>, written as the EJ2 class that styles it.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<FusionToastTone>))]
public enum FusionToastTone
{
    /// <summary>Success.</summary>
    [JsonStringEnumMemberName("e-toast-success")]
    Success,

    /// <summary>A warning.</summary>
    [JsonStringEnumMemberName("e-toast-warning")]
    Warning,

    /// <summary>Danger.</summary>
    [JsonStringEnumMemberName("e-toast-danger")]
    Danger,

    /// <summary>Information.</summary>
    [JsonStringEnumMemberName("e-toast-info")]
    Info,
}
