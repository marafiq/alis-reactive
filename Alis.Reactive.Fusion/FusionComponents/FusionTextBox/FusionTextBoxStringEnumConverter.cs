using System;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

internal sealed class FusionTextBoxStringEnumConverter<T> : JsonStringEnumConverter<T>
    where T : struct, Enum
{
    public FusionTextBoxStringEnumConverter()
        : base(namingPolicy: null, allowIntegerValues: false)
    {
    }
}
