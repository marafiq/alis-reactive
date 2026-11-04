using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Members
{
    internal static class JsonMemberName
    {
        internal static string Of(string memberName) => JsonNamingPolicy.CamelCase.ConvertName(memberName);

        internal static string Of(MemberInfo member) => member.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? Of(member.Name);
    }
}
