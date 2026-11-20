using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Alis.Reactive.Conformance.App.World;

internal static class WorldArea
{
    internal const string Html = "text/html; charset=utf-8";
    private const string DocumentHeader = "Sec-Fetch-Dest";

    internal static void AddWorld(this IServiceCollection services)
    {
        services.AddSingleton<Worlds>();
        services.AddScoped<CurrentWorld>();
        services.AddHttpContextAccessor();
        services.AddSingleton<IDistributedCache>(provider => new WorldCache(
            new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())),
            provider.GetRequiredService<IHttpContextAccessor>()));
        services.AddSingleton<CollectorChanges>();
        services.AddSingleton<ExportedActivities>();
        services.AddSingleton<Collector>();
        services.AddSingleton<SecondOrigin>();
    }

    internal static void UseWorld(this WebApplication app)
        => app.Use(async (context, next) =>
        {
            if (WorldOf(context) is { } key)
            {
                context.RequestServices.GetRequiredService<CurrentWorld>().Open(key, context.RequestServices.GetRequiredService<Worlds>().Of(key));
                Activity.Current?.SetTag(WorldKey.Tag, key);
            }

            await next(context);
        });

    internal static void MapWorld(this WebApplication app)
    {
        app.MapPost(WorldEndpoints.Seed, async (Seed seed, CurrentWorld world, IDistributedCache cache) =>
        {
            world.Data.Take(seed);
            if (seed.CachedRoundsPlanVersion is { } version) await cache.SetStringAsync(CachedRounds.Key, CachedRounds.AsRenderedAtPlanVersion(version));
            if (seed.CachedVitalsPlanVersion is { } vitalsVersion) await cache.SetStringAsync(CachedVitals.Key, CachedVitals.AsRenderedAtPlanVersion(vitalsVersion));
            if (seed.CachedVitalsCutOff is true) await cache.SetStringAsync(CachedVitals.Key, CachedVitals.CutOffInsideAnObject());
        });
        app.MapPost(WorldEndpoints.End, (CurrentWorld world, Worlds worlds) => new WorldEnd { GatesStillParked = worlds.End(world.Key) });
        app.MapPost(WorldEndpoints.ArmGate, (GateOrder order, CurrentWorld world) => world.Data.Arm(order));
        app.MapPost(WorldEndpoints.HeldGate, async (GateOrder order, CurrentWorld world, HttpContext context) =>
        {
            if (world.Data.Held(order) is not { } held) return Results.NotFound($"No gate {WorldData.Named(order.Gate, order.At)} is armed in this world.");

            await held.WaitAsync(context.RequestAborted);
            return Results.Ok();
        });
        app.MapPost(WorldEndpoints.ReleaseGate, (GateOrder order, CurrentWorld world)
            => world.Data.Release(order) ? Results.Ok() : Results.Conflict($"The gate {WorldData.Named(order.Gate, order.At)} holds no request in this world."));
        app.MapPost(WorldEndpoints.HostOrders, async (CurrentWorld world, HttpContext context)
            => await world.Data.HostOrders.Next(context.RequestAborted) is { } elementId ? Results.Text(elementId) : Results.NoContent());
        app.MapPost(WorldEndpoints.HostRestart, async (RestartOrder order, CurrentWorld world, HttpContext context)
            => await world.Data.HostOrders.Restart(order.ElementId, context.RequestAborted) is { Length: > 0 } report ? Results.Text(report, statusCode: StatusCodes.Status409Conflict) : Results.Ok());
        app.MapPost(WorldEndpoints.HostDone, async (CurrentWorld world, HttpRequest request) =>
        {
            using var body = new StreamReader(request.Body);
            return world.Data.HostOrders.Done(await body.ReadToEndAsync()) ? Results.Ok() : Results.Text("No restart is in work in this world.", statusCode: StatusCodes.Status409Conflict);
        });
        app.MapPost(WorldEndpoints.StreamPush, (StreamOrder order, CurrentWorld world)
            => new StreamOrder { Stream = order.Stream, Event = order.Event, Data = order.Data, Count = world.Data.Streams.Of(order.Stream).Push(order.Event, order.Data) });
        app.MapPost(WorldEndpoints.StreamOpen, async (StreamOrder order, CurrentWorld world, HttpContext context)
            => await world.Data.Streams.Of(order.Stream).Holds(order.Count, context.RequestAborted));
        app.MapPost(WorldEndpoints.StreamDrop, (StreamOrder order, CurrentWorld world) => world.Data.Streams.Of(order.Stream).Drop());
        app.MapPost(WorldEndpoints.StreamNextHeld, async (StreamOrder order, CurrentWorld world, HttpContext context)
            => await world.Data.Streams.Of(order.Stream).HoldsTheNext(context.RequestAborted));
        app.MapPost(WorldEndpoints.StreamRefuse, (StreamOrder order, CurrentWorld world) => world.Data.Streams.Of(order.Stream).Refuse());
        app.MapPost(WorldEndpoints.StreamRefused, async (StreamOrder order, CurrentWorld world, HttpContext context)
            => await world.Data.Streams.Of(order.Stream).HasRefused(order.Count, context.RequestAborted));
        app.MapPost(WorldEndpoints.StreamAnswered, async (StreamOrder order, CurrentWorld world, HttpContext context)
            => await world.Data.Streams.Of(order.Stream).HasAnswered(order.Count, context.RequestAborted));
        app.MapPost(WorldEndpoints.StreamReopen, (StreamOrder order, CurrentWorld world) => world.Data.Streams.Of(order.Stream).Reopen());
        app.MapPost(WorldEndpoints.FinallySettle, () => Results.NoContent())
            .AddEndpointFilter((context, next) => AtTheGate(GateName.Of(RealtimeGate.DefaultFinally), context, next));
        app.MapGet(WorldPages.Host, () => Results.Content(HostPage(), Html));
        app.MapGet(WorldPages.Records, (CurrentWorld world) => Results.Content(RecordsPage(world.Data), Html));
        app.MapGet(WorldPages.Word, () => Results.Content(WordPage(), Html));
        app.MapGet(WorldPages.Words, () => Results.Content(WordsPage(), Html));
        app.MapGet(WorldEndpoints.WordAnswer, TheWorldsWord).AddEndpointFilter(AtTheAnswerGate);
        app.MapGet(WorldEndpoints.WordAnswerAgain, TheWorldsWord).AddEndpointFilter(AtTheAnswerGate);
        app.MapPost(WorldEndpoints.Settle, (CurrentWorld world) => world.Data.SettleRefuses ? Results.StatusCode(StatusCodes.Status500InternalServerError) : Results.NoContent())
            .AddEndpointFilter(AtTheSettleGate);
        app.MapPost(WorldEndpoints.Count, (CurrentWorld world) => world.Data.CountRefuses ? Results.StatusCode(StatusCodes.Status500InternalServerError) : Results.Json(3))
            .AddEndpointFilter(AtTheSettleGate);
        app.MapPost(WorldEndpoints.Item, (CurrentWorld world) => world.Data.ItemRefuses ? Results.StatusCode(StatusCodes.Status500InternalServerError) : Results.Json("Item settled"))
            .AddEndpointFilter(AtTheSettleGate);
        app.MapPost(WorldEndpoints.Text, (CurrentWorld world) => world.Data.TextRefuses ? Results.StatusCode(StatusCodes.Status500InternalServerError) : Results.Json("Settled text"))
            .AddEndpointFilter(AtTheSettleGate);
        app.MapPost(WorldEndpoints.HeldCount, (CurrentWorld world) => world.Data.HeldCountRefuses ? Results.StatusCode(StatusCodes.Status500InternalServerError) : Results.Json(42))
            .AddEndpointFilter(AtTheSettleGate);
        app.MapPost(WorldEndpoints.Record, (CurrentWorld world) => Results.Json(new Record { Count = world.Data.RecordCount, Code = "C-750" }))
            .AddEndpointFilter(AtTheSettleGate);
        app.MapPost(WorldEndpoints.Check, (CurrentWorld world) => Results.Json(world.Data.CheckAnswer))
            .AddEndpointFilter(AtTheSettleGate);
    }

    private static IResult TheWorldsWord(CurrentWorld world) => Results.Content(Page("The word", $"<p>{Text(world.Data.Word)}</p>"), Html);

    private static ValueTask<object?> AtTheAnswerGate(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        => AtTheGate(GateName.Of(CompositionGate.Answer), context, next);

    private static ValueTask<object?> AtTheSettleGate(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        => AtTheGate(GateName.Of(RunGate.Settle), context, next);

    internal static async ValueTask<object?> AtTheGate(string gate, EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        await http.RequestServices.GetRequiredService<CurrentWorld>().Data.Reach(gate, http.Request.Path);
        return await next(context);
    }

    private static string? WorldOf(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(WorldKey.Cookie, out var carried) && !string.IsNullOrEmpty(carried)) return carried;
        if (!IsADocuments(context.Request)) return null;

        var key = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(WorldKey.Cookie, key);
        return key;
    }

    internal static bool IsADocuments(HttpRequest request)
        => string.Equals(request.Headers[DocumentHeader].ToString(), "document", StringComparison.Ordinal);

    private static string RecordsPage(WorldData world)
        => Page("Residents", "<h1>Residents</h1><ul>" + string.Concat(world.Residents.Select(resident => $"<li>{Text(resident)}</li>")) + "</ul>");

    private static string HostPage()
        => Page("The host",
            $"<section id=\"callbacks\" data-conformance-callbacks=\"{Text(JsonSerializer.Serialize(new CallbacksStart { OwnNamesGeneration = true }))}\"></section>"
            + "<script src=\"/js/alis-reactive-conformance.js\"></script><script src=\"/_content/AlisReactive/js/alis-reactive.js\"></script>");

    private static string WordPage()
        => Page("The word", $"<a href=\"{WorldEndpoints.WordAnswer}\" target=\"answer\">Read the word</a><iframe id=\"answer\" name=\"answer\" title=\"The word's answer\"></iframe>");

    private static string WordsPage()
        => Page("The words",
            $"<a href=\"{WorldEndpoints.WordAnswer}\" target=\"answer\">Read the word</a><iframe id=\"answer\" name=\"answer\" title=\"The word's answer\"></iframe>"
            + $"<a href=\"{WorldEndpoints.WordAnswerAgain}\" target=\"again\">Read it again</a><iframe id=\"again\" name=\"again\" title=\"The word's answer again\"></iframe>");

    internal static string Page(string title, string body)
        => $"<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>{Text(title)}</title></head><body>{body}</body></html>";

    internal static string Text(string written) => HtmlEncoder.Default.Encode(written);
}
