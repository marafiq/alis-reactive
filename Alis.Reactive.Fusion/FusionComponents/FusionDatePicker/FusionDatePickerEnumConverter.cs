using System;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

internal sealed class FusionDatePickerEnumConverter<T> : JsonStringEnumConverter<T>
    where T : struct, Enum
{
    public FusionDatePickerEnumConverter()
        : base(namingPolicy: null, allowIntegerValues: false)
    {
    }
}
