#if NET10_0_OR_GREATER
using System;

namespace Alis.Reactive.PathsAndWire;

internal static partial class ViewTimeZone
{
    private static partial string IanaIdOf(TimeZoneInfo zone)
    {
        if (zone.HasIanaId) return zone.Id;
        if (TimeZoneInfo.TryConvertWindowsIdToIanaId(zone.Id, out var ianaId)) return ianaId;
        throw new InvalidOperationException(
            $"This request renders under the time zone '{zone.Id}', which .NET converts to no IANA id on this host, so the plan could not name it to the browser; " +
            "choose a time zone TimeZoneInfo.FindSystemTimeZoneById finds.");
    }
}
#endif
