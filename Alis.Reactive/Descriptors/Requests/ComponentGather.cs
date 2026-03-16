using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Requests
{
    public sealed class ComponentGather : GatherItem
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "component";

        public string ComponentId { get; }
        public string Vendor { get; }
        public string Name { get; }
        public string ReadExpr { get; }

        public ComponentGather(string componentId, string vendor, string name, string readExpr)
        {
            ComponentId = componentId;
            Vendor = vendor;
            Name = name;
            ReadExpr = readExpr;
        }
    }
}
