using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Conformance.App;

internal static class RequestZones
{
    private const string Query = "zone";

    internal static void UseRequestZones(this WebApplication app) => app.Use(async (context, next) =>
    {
        var named = context.Request.Query[Query];
        if (named.Count == 0)
        {
            await next(context);
            return;
        }

        var zone = Served(named.ToString());
        if (zone is null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync("<!DOCTYPE html><html lang=\"en\"><head><title>An unserved time zone</title></head><body><p>The zone query names one of America/New_York and Asia/Kolkata, once.</p></body></html>");
            return;
        }

        context.RendersUnder(zone);
        await next(context);
    });

    private static TimeZoneInfo? Served(string name) => name switch
    {
        "America/New_York" or "Asia/Kolkata" => TimeZoneInfo.FindSystemTimeZoneById(name),
        _ => null,
    };
}
