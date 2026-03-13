using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Requests
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(ComponentGather), "component")]
    [JsonDerivedType(typeof(StaticGather), "static")]
    [JsonDerivedType(typeof(AllGather), "all")]
    public abstract class GatherItem
    {
    }

    public sealed class ComponentGather : GatherItem
    {
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

    /// <summary>Plan-driven marker — runtime expands from merged plan.components at gather time.</summary>
    public sealed class AllGather : GatherItem
    {
        internal AllGather()
        {
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
