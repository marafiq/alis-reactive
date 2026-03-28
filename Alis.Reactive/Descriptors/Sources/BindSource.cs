using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Descriptors.Sources
{
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<BindSource>))]
    public abstract class BindSource { }

    internal sealed class EventSource : BindSource
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "event";

        public string Path { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal EventSource(string path)
        {
            Path = path;
        }
    }

    public sealed class ComponentSource : BindSource
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "component";

        public string ComponentId { get; }
        public string Vendor { get; }
        public string ReadExpr { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal ComponentSource(string componentId, string vendor, string readExpr)
        {
            ComponentId = componentId;
            Vendor = vendor;
            ReadExpr = readExpr;
        }
    }
}
