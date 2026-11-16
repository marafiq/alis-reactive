using System.Collections.Concurrent;
using System.Text.Json;
using System.Web;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.World;

internal static class TwinWorlds
{
    private const string DocumentHeader = "Sec-Fetch-Dest";
    private const string Opened = "Alis.Reactive.Conformance.World.Key";

    private static readonly ConcurrentDictionary<string, TwinWorld> ByKey = new(StringComparer.Ordinal);

    internal static JsonSerializerOptions HarnessJson { get; } = new(JsonSerializerDefaults.Web);

    internal static void Open(HttpContextBase context)
    {
        var carried = context.Request.Cookies[WorldKey.Cookie]?.Value;
        if (!string.IsNullOrEmpty(carried))
        {
            context.Items[Opened] = Guid.TryParseExact(carried, "N", out _) ? carried : throw new InvalidOperationException($"The world cookie holds '{carried}', no world's key.");
            return;
        }

        if (!string.Equals(context.Request.Headers[DocumentHeader], "document", StringComparison.Ordinal)) return;

        var key = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Add(new HttpCookie(WorldKey.Cookie, key));
        context.Items[Opened] = key;
    }

    internal static string KeyOf(HttpContextBase context)
        => context.Items[Opened] as string ?? throw new InvalidOperationException("No world is open for this request: a world's own page and the World area are opened in a world.");

    internal static TwinWorld Of(HttpContextBase context) => ByKey.GetOrAdd(KeyOf(context), _ => new TwinWorld());

    internal static TwinWorld? OpenFor(HttpContextBase context)
        => context.Items[Opened] is string key && ByKey.TryGetValue(key, out var world) ? world : null;

    internal static void End(string key)
    {
        if (ByKey.TryRemove(key, out var world)) world.HostOrders.End();
    }
}
