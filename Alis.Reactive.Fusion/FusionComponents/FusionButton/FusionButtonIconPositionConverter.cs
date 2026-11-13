using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

internal sealed class FusionButtonIconPositionConverter : JsonStringEnumConverter<FusionButtonIconPosition>
{
    public FusionButtonIconPositionConverter()
        : base(namingPolicy: null, allowIntegerValues: false)
    {
    }
}
