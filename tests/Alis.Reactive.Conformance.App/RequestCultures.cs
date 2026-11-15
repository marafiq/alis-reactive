using System.Globalization;

namespace Alis.Reactive.Conformance.App;

internal static class RequestCultures
{
    private const string Query = "culture";

    internal static void UseRequestCultures(this WebApplication app) => app.Use(async (context, next) =>
    {
        var named = context.Request.Query[Query];
        var culture = named.Count == 0 ? CultureInfo.GetCultureInfo("en-US") : Served(named.ToString());
        if (culture is null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync("<!DOCTYPE html><html lang=\"en\"><head><title>An unserved culture</title></head><body><p>The culture query names one of en-US, de-DE, tr-TR, haw-US, de-DE_phoneb and invariant, once.</p></body></html>");
            return;
        }

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        await next(context);
    });

    private static CultureInfo? Served(string name) => name switch
    {
        "en-US" or "de-DE" or "tr-TR" or "haw-US" or "de-DE_phoneb" => CultureInfo.GetCultureInfo(name),
        "invariant" => CultureInfo.InvariantCulture,
        _ => null,
    };
}
