#if NET48
using System;

namespace Alis.Reactive.PathsAndWire;

internal static partial class ViewTimeZone
{
    private static partial string IanaIdOf(TimeZoneInfo zone)
    {
        if (WindowsZones.Names(zone.Id, out var ianaId)) return ianaId;
        throw new InvalidOperationException(
            $"This request renders under the time zone '{zone.Id}', which CLDR's table of Windows time zones at {WindowsZones.Release} does not hold, so the plan could not name it to the browser; " +
            "choose a time zone TimeZoneInfo.FindSystemTimeZoneById finds by its Windows id, or take the AlisReactive release whose table holds it.");
    }
}
#endif
