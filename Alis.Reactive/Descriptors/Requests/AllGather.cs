using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Requests
{
    /// <summary>Plan-driven marker — runtime expands from merged plan.components at gather time.</summary>
    public sealed class AllGather : GatherItem
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "all";

        internal AllGather()
        {
        }
    }
}
