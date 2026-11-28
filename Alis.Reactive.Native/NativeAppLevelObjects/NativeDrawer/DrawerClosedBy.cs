using System.Text.Json.Serialization;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// What closes a <see cref="NativeDrawer"/>, written by the drawer code's name.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DrawerClosedBy>))]
public enum DrawerClosedBy
{
    /// <summary>The Escape key, pressed on the page.</summary>
    [JsonStringEnumMemberName("escape")]
    Escape,

    /// <summary>The drawer's close button.</summary>
    [JsonStringEnumMemberName("closeButton")]
    CloseButton,

    /// <summary>A call of <see cref="NativeDrawer.Close"/>.</summary>
    [JsonStringEnumMemberName("close")]
    Close,
}
