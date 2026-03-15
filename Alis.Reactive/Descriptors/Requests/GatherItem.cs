using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Descriptors.Requests
{
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<GatherItem>))]
    public abstract class GatherItem
    {
    }
}
