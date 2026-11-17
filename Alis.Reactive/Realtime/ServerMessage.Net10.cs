#if NET10_0_OR_GREATER
using System;
using System.Text.Json;

namespace Alis.Reactive.Realtime;

/// <summary>
/// A message of a stream's declared event, which the endpoint writes with <see cref="ServerEvents.Write"/>.
/// </summary>
public sealed class ServerMessage
{
    private readonly Func<JsonSerializerOptions, string> _data;

    internal ServerMessage(string name, Func<JsonSerializerOptions, string> data)
    {
        Name = name;
        _data = data;
    }

    internal string Name { get; }

    internal string DataUnder(JsonSerializerOptions options) => _data(options);
}
#endif
