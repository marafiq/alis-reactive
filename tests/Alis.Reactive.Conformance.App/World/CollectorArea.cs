using System.Runtime.CompilerServices;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.World;

internal static class CollectorArea
{
    private const string Script = "/js/world/collector.js";
    private const string Search = "search";
    private const string SearchForm = "<form method=\"get\" action=\"" + WorldPages.Collector + "\"><label>Search <input type=\"search\" name=\"" + Search + "\"></label> <button>Search</button></form>";

    internal static void UseSecondOrigin(this WebApplication app)
        => app.Use(async (context, next) =>
        {
            var request = context.Request;
            if (context.RequestServices.GetRequiredService<SecondOrigin>().Received(request) && request.Query[WorldKey.AtTheSecondOrigin].ToString() is { Length: > 0 } key)
            {
                var world = context.RequestServices.GetRequiredService<Worlds>().Of(key);
                context.RequestServices.GetRequiredService<CurrentWorld>().Open(key, world);
                world.SecondOriginReceived(request.Method + " " + request.Path);
            }

            await next(context);
        });

    internal static void MapCollector(this WebApplication app)
    {
        app.MapPost(WorldEndpoints.CollectorTraces, (OtlpExport export, CurrentWorld world, CollectorChanges changes) =>
        {
            foreach (var resource in export.ResourceSpans)
            {
                foreach (var scope in resource.ScopeSpans)
                {
                    foreach (var span in scope.Spans) world.Data.Collected(CollectedSpan.Of(span, scope.Scope.Name));
                }
            }

            changes.Changed();
            return Results.Ok();
        });
        app.MapGet(WorldPages.Collector, (CurrentWorld world, Collector collector, HttpRequest request)
            => Results.Content(WorldArea.Page("The collector", "<h1>Collector</h1>" + SearchForm + "<div id=\"spans\">" + collector.LinesOf(world.Key, SearchOf(request)) + $"</div><script src=\"{Script}\"></script>"), WorldArea.Html));
        app.MapGet(WorldEndpoints.CollectorLines, (CurrentWorld world, Collector collector, CollectorChanges changes, HttpContext context)
            => TypedResults.ServerSentEvents(LinesAsTheyChange(world.Key, SearchOf(context.Request), collector, changes, context.RequestAborted)));
        app.MapGet(WorldPages.Echo, (CurrentWorld world, Collector collector)
            => Results.Content(WorldArea.Page("The echo", "<h1>Echo</h1>" + ListOf(world.Data.Attempts.Select((attempt, at) => EchoOf(attempt, at + 1, world.Key, collector)))), WorldArea.Html));
        app.MapGet(WorldPages.SecondOriginLog, (CurrentWorld world)
            => Results.Content(WorldArea.Page("The second origin", "<h1>Second origin</h1>" + ListOf(world.Data.SecondOriginLog)), WorldArea.Html));
        app.MapGet(WorldEndpoints.PharmacyStock, (HttpRequest request, CurrentWorld world, Collector collector) => Results.Text(TraceEcho.Traceparent(request, world.Key, collector)))
            .RequireCors(SecondOrigin.Policy)
            .AddEndpointFilter(AtTheRespondGate);
    }

    private static async IAsyncEnumerable<string> LinesAsTheyChange(string world, string search, Collector collector, CollectorChanges changes, [EnumeratorCancellation] CancellationToken stop)
    {
        while (!stop.IsCancellationRequested)
        {
            var next = changes.Next;
            yield return collector.LinesOf(world, search);
            await next.WaitAsync(stop);
        }
    }

    private static string EchoOf(HeardAttempt attempt, int number, string world, Collector collector)
        => $"attempt {number}: {TraceEcho.Traceparent(attempt.Traceparent, world, collector)}, {TraceEcho.Baggage(attempt.Baggage)}";

    private static string SearchOf(HttpRequest request) => request.Query[Search].ToString();

    private static string ListOf(IEnumerable<string> lines) => "<ol>" + string.Concat(lines.Select(line => "<li>" + WorldArea.Text(line) + "</li>")) + "</ol>";

    private static ValueTask<object?> AtTheRespondGate(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        => WorldArea.AtTheGate(GateName.Of(TracingGate.Respond), context, next);
}
