using System.Globalization;
using System.Web;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Conformance.App;

internal static class RequestCulturesAndZones
{
    private const string CultureQuery = "culture";
    private const string ZoneQuery = "zone";
    private const int BadRequest = 400;

    internal static bool Take(HttpContextBase context)
    {
        var cultures = context.Request.QueryString.GetValues(CultureQuery);
        var culture = cultures is null ? CultureInfo.GetCultureInfo("en-US") : cultures is [var named] ? ServedCulture(named) : null;
        if (culture is null)
            return Refused(context, "An unserved culture", "The culture query names one of en-US, de-DE, tr-TR, haw-US, de-DE_phoneb and invariant, once.");

        var zones = context.Request.QueryString.GetValues(ZoneQuery);
        var zone = zones is null ? null : zones is [var chosen] ? ServedZone(chosen) : null;
        if (zones is not null && zone is null)
            return Refused(context, "An unserved time zone", "The zone query names one of America/New_York and Asia/Kolkata, once.");

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        if (zone is not null) context.RendersUnder(zone);
        return true;
    }

    private static CultureInfo? ServedCulture(string name) => name switch
    {
        "en-US" or "de-DE" or "tr-TR" or "haw-US" or "de-DE_phoneb" => CultureInfo.GetCultureInfo(name),
        "invariant" => CultureInfo.InvariantCulture,
        _ => null,
    };

    private static TimeZoneInfo? ServedZone(string name) => name switch
    {
        "America/New_York" => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"),
        "Asia/Kolkata" => TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"),
        _ => null,
    };

    private static bool Refused(HttpContextBase context, string title, string reason)
    {
        context.Response.StatusCode = BadRequest;
        context.Response.TrySkipIisCustomErrors = true;
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.Write($"<!DOCTYPE html><html lang=\"en\"><head><title>{title}</title></head><body><p>{reason}</p></body></html>");
        return false;
    }
}
