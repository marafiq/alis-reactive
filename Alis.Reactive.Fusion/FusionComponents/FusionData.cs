using Newtonsoft.Json.Linq;

namespace Alis.Reactive.Fusion.FusionComponents;

internal static class FusionData
{
    internal static JRaw AsWritten(string json) => new(json);
}
