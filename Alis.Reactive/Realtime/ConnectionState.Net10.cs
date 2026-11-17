#if NET10_0_OR_GREATER
using System.Text.Json.Serialization;

namespace Alis.Reactive.Realtime;

/// <summary>
/// The state of the page's connection to a stream.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ConnectionState>))]
public enum ConnectionState
{
    /// <summary>The connection has not opened yet.</summary>
    [JsonStringEnumMemberName("connecting")]
    Connecting,

    /// <summary>The connection is open.</summary>
    [JsonStringEnumMemberName("open")]
    Open,

    /// <summary>The connection dropped and the browser is opening it again.</summary>
    [JsonStringEnumMemberName("reconnecting")]
    Reconnecting,

    /// <summary>The browser gave up on the connection, or a step closed it.</summary>
    [JsonStringEnumMemberName("closed")]
    Closed,
}
#endif
