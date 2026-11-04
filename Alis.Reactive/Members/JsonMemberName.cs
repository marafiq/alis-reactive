using System.Text.Json;

namespace Alis.Reactive.Members
{
    internal static class JsonMemberName
    {
        internal static string Of(string memberName) => JsonNamingPolicy.CamelCase.ConvertName(memberName);
    }
}
