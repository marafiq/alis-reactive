#if NET48
using System;
using System.Web;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using TimeZone = Alis.Reactive.Schema.TimeZone;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The time zone a request's views render under: the app's choice for each request, as its culture is.
/// </summary>
public static class RequestTimeZone
{
    private static readonly object Chosen = new();

    /// <summary>Chooses the time zone every view of this request renders under.</summary>
    /// <remarks>
    /// The browser shows each <see cref="DateTimeOffset"/> a view's plan reads at this zone's wall time and offset for
    /// its own day, and sends a time a person picks at that offset. Choose it before the request's first view
    /// renders, in an action filter or the action: a view that rendered before the choice keeps what stood then. A
    /// request that chooses none shows and sends each one on the browser's own clock.
    /// </remarks>
    /// <param name="context">The request.</param>
    /// <param name="timeZone">The zone, one <see cref="TimeZoneInfo.FindSystemTimeZoneById(string)"/> finds by its Windows id.</param>
    public static void RendersUnder(this HttpContextBase context, TimeZoneInfo timeZone)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        context.Items[Chosen] = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
    }

    internal static TimeZone Of(HttpContextBase context)
        => context.Items[Chosen] is TimeZoneInfo zone ? ViewTimeZone.Of(zone) : ClockTimeZone.Instance;
}
#endif
