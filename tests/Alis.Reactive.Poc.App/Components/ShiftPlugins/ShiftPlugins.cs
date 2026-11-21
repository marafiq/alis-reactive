using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.Components;
using Alis.Reactive.Values;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/ShiftPlugins/shift-plugins", "ShiftLog")]
public sealed class ShiftLog : Plugin
{
    public ShiftLog()
        : base("shift-log")
    {
    }

    public ReadOnly<int> Count => ReadOnly<int>();

    public ReadOnly<string> LastNote => ReadOnly<string>();

    public Property<int> Limit => Property<int>();

    public Event<ShiftNoted> Noted => Event<ShiftNoted>();

    public void Add(Arg<string> note) => Method([note]);

    public MethodResult<bool> IsFull() => Method<bool>();
}

public sealed class ShiftNoted : JsObject
{
    public ReadOnly<NotedDetail?> Detail => ReadOnly<NotedDetail?>();
}

public sealed record NotedDetail(string Note, int Count);

[Mirrors("Components/ShiftPlugins/shift-plugins", "initials")]
public sealed class Initials : Plugin
{
    public Initials()
        : base("initials")
    {
    }

    public MethodResult<string> Of(Arg<string> name) => CallItself<string>([name]);
}

[Mirrors("Components/ShiftPlugins/shift-plugins", "NightClock")]
public sealed class NightClock : Plugin
{
    public NightClock()
        : base("night-clock", EventListening.CallbackProperties)
    {
    }

    public Event<ClockStruck> Struck => Event<ClockStruck>(name: "onstruck");

    public void Strike() => Method();
}

public sealed class ClockStruck : JsObject
{
    public ReadOnly<string> Hour => ReadOnly<string>();
}

[Mirrors("Components/ShiftPlugins/shift-plugins", "BedStore")]
public sealed class BedStore : Plugin
{
    public BedStore()
        : base("bed-store", EventListening.ReturnedUnsubscriber("subscribe"))
    {
    }

    public Event<BedTaken> Assigned => Event<BedTaken>();

    public void Assign(Arg<string> bed) => Method([bed]);
}

public sealed class BedTaken : JsObject
{
    public ReadOnly<string> Bed => ReadOnly<string>();
}

[Mirrors("Components/ShiftPlugins/shift-plugins", "VisitorBook")]
public sealed class VisitorBook : Plugin
{
    public VisitorBook()
        : base("visitor-book")
    {
    }

    public ReadOnly<string> LastVisitor => ReadOnly<string>();

    public Event<BookSigned> Signed => Event<BookSigned>();

    public void Sign(Arg<string> visitor) => Method([visitor]);
}

public sealed class BookSigned : JsObject
{
    public ReadOnly<SignedDetail?> Detail => ReadOnly<SignedDetail?>();
}

public sealed record SignedDetail(string Visitor);

[SuppressMessage("Alis.Reactive.Contract", "ALIS014", Justification = "Drift on purpose: a_plugin_provided_as_a_number_stops_each_step_that_reads_a_member_a_number_lacks gives the page a number, which no TypeScript type declares.")]
public sealed class WardLantern : Plugin
{
    public WardLantern()
        : base("ward-lantern")
    {
    }

    public ReadOnly<string> Colour => ReadOnly<string>();
}

[SuppressMessage("Alis.Reactive.Contract", "ALIS014", Justification = "Drift on purpose: a_plugin_named_with_a_dot_is_refused_where_the_view_renders refuses it before any script could provide it.")]
public sealed class DottedLog : Plugin
{
    public DottedLog()
        : base("alis.log")
    {
    }

    public ReadOnly<int> Count => ReadOnly<int>();
}
