using System;
using Alis.Reactive.Schema;
using TimeZone = Alis.Reactive.Schema.TimeZone;

namespace Alis.Reactive.PathsAndWire;

internal static partial class ViewTimeZone
{
    internal static TimeZone Of(TimeZoneInfo zone) => new ZoneTimeZone(IanaIdOf(zone));

    private static partial string IanaIdOf(TimeZoneInfo zone);
}
