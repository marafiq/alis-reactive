using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.ModuleTests.Plugins;

public sealed record NightRound(string Ward);

public sealed class ShiftLog : Plugin
{
    public ShiftLog()
        : base("shiftlog")
    {
    }

    public ReadOnly<int> Entries => ReadOnly<int>();

    public Property<int> Count => Property<int>();

    public Property<string> Note => Property<string>();

    public void Add(Arg<string> note) => Method([note]);

    public MethodResult<int> Total() => Method<int>();
}

public sealed class VisitorBook : Plugin
{
    public VisitorBook()
        : base("visitorbook")
    {
    }

    public Property<string> Note => Property<string>();

    public Event Signed => Event();

    public Event<Visit> Visited => Event<Visit>();
}

public sealed class Visit : JsObject
{
    public ReadOnly<string> Room => ReadOnly<string>();
}

public sealed class NightClock : Plugin
{
    public NightClock()
        : base("nightclock", EventListening.CallbackProperties)
    {
    }

    public Property<string> Shown => Property<string>();

    public Event Struck => Event(name: "onStruck");
}

public sealed class NightClockFace : Plugin
{
    public NightClockFace()
        : base("nightclock")
    {
    }

    public ReadOnly<string> Hour => ReadOnly<string>();
}

public sealed class NightChime : Plugin
{
    private static readonly string TheClocksName = string.Concat("night", "clock");

    public NightChime()
        : base(TheClocksName)
    {
    }

    public Event Chimed => Event();
}

public sealed class BedStore : Plugin
{
    public BedStore()
        : base("bedstore", EventListening.ReturnedUnsubscriber("subscribe"))
    {
    }

    public Property<int> Free => Property<int>();

    public Event<BedChange> Changed => Event<BedChange>();
}

public sealed class BedChange : JsObject
{
    public ReadOnly<int> Bed => ReadOnly<int>();
}

public sealed class WardDesk : Plugin
{
    public WardDesk()
        : base("ward-desk_2", EventListening.Methods("hear", "forget"))
    {
    }

    public Property<string> Note => Property<string>();

    public Event Called => Event();
}

public sealed class WardDeskBell : Plugin
{
    public WardDeskBell()
        : base("ward-desk_2", EventListening.Methods("hear", "forget"))
    {
    }

    public Event Rang => Event();
}

public sealed class Initials : Plugin
{
    public Initials()
        : base("initials")
    {
    }

    public void Refresh() => CallItself();

    public void Refresh(Arg<string> name) => CallItself([name]);

    public MethodResult<string> Of(Arg<string> name) => CallItself<string>([name]);
}

public sealed class DottedLog : Plugin
{
    private static readonly string TheFrameworksName = string.Join(".", "alis", "log");

    public DottedLog()
        : base(TheFrameworksName)
    {
    }

    public Property<int> Count => Property<int>();
}

public sealed class NumberedLog : Plugin
{
    private static readonly string ANumberFirst = string.Join("-", "2nd", "log");

    public NumberedLog()
        : base(ANumberFirst)
    {
    }

    public Property<int> Count => Property<int>();
}

public sealed class ClockFace : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Define("clock-face", VendorLookup.ByPlugin("nightclock"), "on", "off");

    public Property<string> Hour => Property<string>();
}

internal static class RoundPlans
{
    internal static string JsonOf(IHtmlContent script)
        => Views.TextOf(script)["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
}
