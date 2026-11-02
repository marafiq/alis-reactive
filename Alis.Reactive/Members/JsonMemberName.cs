using System.Text.Json;

namespace Alis.Reactive
{
    // The name a member has in the JSON the browser reads: System.Text.Json's camel-case naming, which
    // ASP.NET Core, the plan serializer and SignalR use ("MRN" is "mrn", "URLPath" is "urlPath").
    internal static class JsonMemberName
    {
        internal static string Of(string memberName) => JsonNamingPolicy.CamelCase.ConvertName(memberName);
    }
}
