using System;
using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.ModuleTests.Declarations;

public sealed record Infusion(string Ward);

public sealed class InfusionPump : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<int> FlowRate => ReadOnly<int>();

    public ReadOnly<int> Volume => ReadOnly<int>(name: "VTBI");

    public ReadOnly<string?> Alarm => ReadOnly<string?>(optional: true);

    public Property<int> Rate => Property<int>();

    public Property<DateTime> StartsAt => Property<DateTime>();

    public Property<DateTime?> StopsAt => Property<DateTime?>();

    public Property<string?> Label => Property<string?>(optional: true);

    public Property<int?> Limit => Property<int?>();

    public WriteOnly<int> Bolus => WriteOnly<int>();

    public WriteOnly<int?> Lock => WriteOnly<int?>(optional: true);

    public WriteOnly<DateTimeOffset> PausedUntil => WriteOnly<DateTimeOffset>();
}
