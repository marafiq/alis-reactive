using System.Text.Json.Serialization;

namespace Alis.Reactive.Grammar
{
    [JsonConverter(typeof(CamelCaseEnum<PathMembers>))]
    internal enum PathMembers
    {
        Required,
        Optional,
    }
}
