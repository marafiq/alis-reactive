using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

internal sealed class FusionCheckBoxLabelPositionConverter : JsonStringEnumConverter<FusionCheckBoxLabelPosition>
{
    public FusionCheckBoxLabelPositionConverter()
        : base(namingPolicy: null, allowIntegerValues: false)
    {
    }
}
