using Alis.Reactive.Components;
using Alis.Reactive.Values;

namespace Alis.Reactive.Poc.App.Components;

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

public sealed class Initials : Plugin
{
    public Initials()
        : base("initials")
    {
    }

    public MethodResult<string> Of(Arg<string> name) => CallItself<string>([name]);
}

public sealed class NightClock : Plugin
{
    public NightClock()
        : base("night-clock", PluginEvents.CallbackProperties)
    {
    }

    public Event<ClockStruck> Struck => Event<ClockStruck>(name: "onstruck");

    public void Strike() => Method();
}

public sealed class ClockStruck : JsObject
{
    public ReadOnly<string> Hour => ReadOnly<string>();
}

public sealed class BedStore : Plugin
{
    public BedStore()
        : base("bed-store", PluginEvents.ReturnedUnsubscriber("subscribe"))
    {
    }

    public Event<BedTaken> Assigned => Event<BedTaken>();

    public void Assign(Arg<string> bed) => Method([bed]);
}

public sealed class BedTaken : JsObject
{
    public ReadOnly<string> Bed => ReadOnly<string>();
}

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

public sealed class WardLantern : Plugin
{
    public WardLantern()
        : base("ward-lantern")
    {
    }

    public ReadOnly<string> Colour => ReadOnly<string>();
}

public sealed class DottedLog : Plugin
{
    public DottedLog()
        : base("alis.log")
    {
    }

    public ReadOnly<int> Count => ReadOnly<int>();
}
