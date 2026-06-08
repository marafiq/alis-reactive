using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp
{
    /// <summary>
    /// Resolves Sandbox controller endpoints through MVC routing instead of hard-coded URL
    /// strings. Reactive-plan calls still receive a string, but it is a resolved one: route
    /// template changes flow through automatically, and an unresolved route fails loudly at
    /// render time rather than 404-ing silently at runtime.
    /// </summary>
    public static class SandboxRouteExtensions
    {
        /// <summary>Resolves a parameterless Sandbox-area action URL.</summary>
        public static string SandboxRoute(this IUrlHelper url, string action, string controller)
            => url.Action(action, controller, new { area = "Sandbox" })
               ?? throw new InvalidOperationException(
                   $"Could not resolve Sandbox route {controller}.{action}.");

        /// <summary>Resolves a Sandbox-area action URL with route values (for templated parameters).</summary>
        public static string SandboxRoute(this IUrlHelper url, string action, string controller, object routeValues)
        {
            var values = new RouteValueDictionary(routeValues) { ["area"] = "Sandbox" };
            return url.Action(action, controller, values)
                   ?? throw new InvalidOperationException(
                       $"Could not resolve Sandbox route {controller}.{action}.");
        }

        /// <summary>
        /// Resolves a Sandbox-area route as a reactive-plan URL template, keeping the named
        /// <c>{param}</c> placeholders the runtime substitutes (via <c>RouteParam(...)</c>).
        /// The route prefix is still MVC-resolved, so template changes flow through, but the
        /// placeholders survive. Sentinel values satisfy <c>{id:int}</c>-style constraints.
        /// </summary>
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
