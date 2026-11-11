using System.Text.Encodings.Web;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.World;

internal static class WorldArea
{
    private const string Html = "text/html; charset=utf-8";

    internal static void AddWorld(this IServiceCollection services)
    {
        services.AddSingleton<Worlds>();
        services.AddScoped<CurrentWorld>();
    }

    internal static void UseWorld(this WebApplication app)
        => app.Use(async (context, next) =>
        {
            var worlds = context.RequestServices.GetRequiredService<Worlds>();
            if (!context.Request.Cookies.TryGetValue(WorldKey.Cookie, out var key) || string.IsNullOrEmpty(key))
            {
                key = Guid.NewGuid().ToString("N");
                context.Response.Cookies.Append(WorldKey.Cookie, key);
            }

            context.RequestServices.GetRequiredService<CurrentWorld>().Open(key, worlds.Of(key));
            await next(context);
        });

    internal static void MapWorld(this WebApplication app)
    {
        app.MapPost(WorldEndpoints.Seed, (Seed seed, CurrentWorld world) => world.Data.Take(seed));
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

    private static string RecordsPage(WorldData world)
        => Page("Residents", "<h1>Residents</h1><ul>" + string.Concat(world.Residents.Select(resident => $"<li>{Text(resident)}</li>")) + "</ul>");

    private static string WordPage()
        => Page("The word", $"<a href=\"{WorldEndpoints.WordAnswer}\" target=\"answer\">Read the word</a><iframe id=\"answer\" name=\"answer\" title=\"The word's answer\"></iframe>");

    private static string Page(string title, string body)
        => $"<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>{Text(title)}</title></head><body>{body}</body></html>";

    private static string Text(string written) => HtmlEncoder.Default.Encode(written);
}
