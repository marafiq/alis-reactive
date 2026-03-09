using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Requests
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(ComponentGather), "component")]
    [JsonDerivedType(typeof(AllGather), "all")]
    [JsonDerivedType(typeof(StaticGather), "static")]
    public abstract class GatherItem
    {
    }

    public sealed class ComponentGather : GatherItem
    {
        public string ComponentId { get; }
        public string Vendor { get; }
        public string Name { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ReadExpr { get; }

        public ComponentGather(string componentId, string vendor, string name, string? readExpr = null)
        {
            ComponentId = componentId;
            Vendor = vendor;
            Name = name;
            ReadExpr = readExpr;
        }
    }

    public sealed class AllGather : GatherItem
    {
        public string FormId { get; }

        public AllGather(string formId)
        {
            FormId = formId;
        }
    }

    public sealed class StaticGather : GatherItem
    {
        public string Param { get; }
        public object Value { get; }

        public StaticGather(string param, object value)
        {
            Param = param;
            Value = value;
        }
    }
}
