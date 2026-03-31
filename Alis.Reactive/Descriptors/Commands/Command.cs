using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Descriptors.Commands
{
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<Command>))]
    public abstract class Command
    {
    }
}
