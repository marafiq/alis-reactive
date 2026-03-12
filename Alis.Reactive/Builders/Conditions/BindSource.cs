using System.Text.Json.Serialization;

namespace Alis.Reactive.Builders.Conditions
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(EventSource), "event")]
    [JsonDerivedType(typeof(ComponentSource), "component")]
    public abstract class BindSource { }

    public sealed class EventSource : BindSource
    {
        public string Path { get; }

        public EventSource(string path)
        {
            Path = path;
        }
    }

    public sealed class ComponentSource : BindSource
    {
        public string ComponentId { get; }
        public string Vendor { get; }
        public string ReadExpr { get; }

        public ComponentSource(string componentId, string vendor, string readExpr)
        {
            ComponentId = componentId;
            Vendor = vendor;
            ReadExpr = readExpr;
        }
    }
}
