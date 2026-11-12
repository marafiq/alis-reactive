using System.Text.Encodings.Web;
using Alis.Reactive.Conformance.World;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Alis.Reactive.Conformance.App.World;

internal static class WorldArea
{
    private const string Html = "text/html; charset=utf-8";
    private const string DocumentHeader = "Sec-Fetch-Dest";

    internal static void AddWorld(this IServiceCollection services)
    {
        services.AddSingleton<Worlds>();
        services.AddScoped<CurrentWorld>();
        services.AddHttpContextAccessor();
        services.AddSingleton<IDistributedCache>(provider => new WorldCache(
            new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())),
            provider.GetRequiredService<IHttpContextAccessor>()));
    }

    internal static void UseWorld(this WebApplication app)
        => app.Use(async (context, next) =>
        {
            if (WorldOf(context) is { } key)
                context.RequestServices.GetRequiredService<CurrentWorld>().Open(key, context.RequestServices.GetRequiredService<Worlds>().Of(key));
            await next(context);
        });

    internal static void MapWorld(this WebApplication app)
    {
        app.MapPost(WorldEndpoints.Seed, async (Seed seed, CurrentWorld world, IDistributedCache cache) =>
        {
            world.Data.Take(seed);
            if (seed.CachedRoundsPlanVersion is { } version) await cache.SetStringAsync(CachedRounds.Key, CachedRounds.AsRenderedAtPlanVersion(version));
        });
        app.MapPost(WorldEndpoints.End, (CurrentWorld world, Worlds worlds) => new WorldEnd { GatesStillParked = worlds.End(world.Key) });
        app.MapPost(WorldEndpoints.ArmGate, (GateOrder order, CurrentWorld world) => world.Data.Arm(order.Gate));
        app.MapPost(WorldEndpoints.HeldGate, async (GateOrder order, CurrentWorld world, HttpContext context) =>
        {
            if (world.Data.Held(order.Gate) is not { } held) return Results.NotFound($"No gate {order.Gate} is armed in this world.");

            await held.WaitAsync(context.RequestAborted);
            return Results.Ok();
        });
        app.MapPost(WorldEndpoints.ReleaseGate, (GateOrder order, CurrentWorld world)
            => world.Data.Release(order.Gate) ? Results.Ok() : Results.Conflict($"The gate {order.Gate} holds no request in this world."));
        app.MapGet(WorldPages.Records, (CurrentWorld world) => Results.Content(RecordsPage(world.Data), Html));
        app.MapGet(WorldPages.Word, () => Results.Content(WordPage(), Html));
        app.MapGet(WorldEndpoints.WordAnswer, (CurrentWorld world) => Results.Content(Page("The word", $"<p>{Text(world.Data.Word)}</p>"), Html))
            .AddEndpointFilter(async (context, next) =>
            {
                await context.HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Reach(GateName.Of(CompositionGate.Answer));
                return await next(context);
            });
    }

    private static string? WorldOf(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(WorldKey.Cookie, out var carried) && !string.IsNullOrEmpty(carried)) return carried;
        if (!IsADocuments(context.Request)) return null;

        var key = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(WorldKey.Cookie, key);
        return key;
    }

    private static bool IsADocuments(HttpRequest request)
        => string.Equals(request.Headers[DocumentHeader].ToString(), "document", StringComparison.Ordinal);

    private static string RecordsPage(WorldData world)
        => Page("Residents", "<h1>Residents</h1><ul>" + string.Concat(world.Residents.Select(resident => $"<li>{Text(resident)}</li>")) + "</ul>");

    private static string WordPage()
        => Page("The word", $"<a href=\"{WorldEndpoints.WordAnswer}\" target=\"answer\">Read the word</a><iframe id=\"answer\" name=\"answer\" title=\"The word's answer\"></iframe>");

    private static string Page(string title, string body)
        => $"<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>{Text(title)}</title></head><body>{body}</body></html>";

    private static string Text(string written) => HtmlEncoder.Default.Encode(written);
}
