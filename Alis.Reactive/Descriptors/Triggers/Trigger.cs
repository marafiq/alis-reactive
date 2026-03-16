using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Descriptors.Triggers
{
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<Trigger>))]
    public abstract class Trigger
    {
    }
}
