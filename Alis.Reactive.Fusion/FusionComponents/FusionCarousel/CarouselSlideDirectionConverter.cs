using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

internal sealed class CarouselSlideDirectionConverter : JsonStringEnumConverter<FusionCarousel.CarouselSlideDirection>
{
    public CarouselSlideDirectionConverter()
        : base(namingPolicy: null, allowIntegerValues: false)
    {
    }
}
