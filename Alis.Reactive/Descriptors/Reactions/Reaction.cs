using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Descriptors.Reactions
{
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<Reaction>))]
    public abstract class Reaction
    {
    }
}
