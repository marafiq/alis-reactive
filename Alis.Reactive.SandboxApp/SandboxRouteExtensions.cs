using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp
{
    public static class SandboxRouteExtensions
    {
        public static string SandboxRoute(this IUrlHelper url, string action, string controller)
            => url.Action(action, controller, new { area = "Sandbox" })
               ?? throw new InvalidOperationException(
                   $"Could not resolve Sandbox route {controller}.{action}.");

        public static string SandboxRoute(this IUrlHelper url, string action, string controller, object routeValues)
        {
            var values = new RouteValueDictionary(routeValues) { ["area"] = "Sandbox" };
            return url.Action(action, controller, values)
                   ?? throw new InvalidOperationException(
                       $"Could not resolve Sandbox route {controller}.{action}.");
        }

        public static string SandboxRouteTemplate(this IUrlHelper url, string action, string controller, params string[] templateParams)
        {
            var values = new RouteValueDictionary { ["area"] = "Sandbox" };
            var sentinels = new (string Name, string Token)[templateParams.Length];
            for (var i = 0; i < templateParams.Length; i++)
            {
                var token = (2147483640 - i).ToString();
                values[templateParams[i]] = token;
                sentinels[i] = (templateParams[i], token);
            }

            var resolved = url.Action(action, controller, values)
                ?? throw new InvalidOperationException(
                    $"Could not resolve Sandbox route template {controller}.{action}.");

            foreach (var (name, token) in sentinels)
            {
                resolved = resolved.Replace(token, "{" + name + "}");
            }
            return resolved;
        }
    }
}
