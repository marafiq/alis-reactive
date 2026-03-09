using System.Text.Json.Serialization;

namespace Alis.Reactive.Builders.Conditions
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(EventSource), "event")]
    public abstract class BindSource { }

    public sealed class EventSource : BindSource
    {
        public string Path { get; }

        public EventSource(string path)
        {
            Path = path;
        }
    }
}
