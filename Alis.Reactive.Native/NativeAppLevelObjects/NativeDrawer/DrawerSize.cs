using System.Text.Json.Serialization;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// How wide a <see cref="NativeDrawer"/> opens, written by the drawer code's name.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DrawerSize>))]
public enum DrawerSize
{
    /// <summary>Small.</summary>
    [JsonStringEnumMemberName("sm")]
    Sm,

    /// <summary>Medium, the size from the render and after each close.</summary>
    [JsonStringEnumMemberName("md")]
    Md,

    /// <summary>Large.</summary>
    [JsonStringEnumMemberName("lg")]
    Lg,
}
