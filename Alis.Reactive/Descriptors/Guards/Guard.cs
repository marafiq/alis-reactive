using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Descriptors.Guards
{
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<Guard>))]
    public abstract class Guard { }
}
