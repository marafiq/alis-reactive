using System;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

internal sealed class FusionDropDownListStringEnumConverter<T> : JsonStringEnumConverter<T>
    where T : struct, Enum
{
    public FusionDropDownListStringEnumConverter()
        : base(namingPolicy: null, allowIntegerValues: false)
    {
    }
}
