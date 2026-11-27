using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

internal sealed class FusionRadioButtonLabelPositionConverter : JsonStringEnumConverter<FusionRadioButtonLabelPosition>
{
    public FusionRadioButtonLabelPositionConverter()
        : base(namingPolicy: null, allowIntegerValues: false)
    {
    }
}
