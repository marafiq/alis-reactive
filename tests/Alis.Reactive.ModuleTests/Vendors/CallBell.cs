using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.ModuleTests.Vendors;

public sealed record Ward(string Name);

public abstract class CallBell : Component
{
    public Property<string?> Text => Property<string?>();
    public Property<int> Count => Property<int>();
    public Event<Ring> Rang => Event<Ring>();
    public Event<Ring> Urgent => Event<Ring>(name: "ring-now");
    public Event Silenced => Event();
    public Event Cleared => Event(name: "clear-all");
    public Event<Search> Searching => Event<Search>();
    public EventListening HeardBy => Listening;
    public ReadOnly<string> MisplacedArgument => Argument<string>(0);
    public void MisplacedCall() => MethodAt(1);
    public void MisplacedCallWith(Arg<string> text) => MethodAt(1, [text]);
}

public sealed class NativeBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.Native;
}

public sealed class PathBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.Define("bell", "devices.2.bell", "hear", "forget", elementPath: "host.0");
}

public sealed class CallbackBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.DefineCallbackProperties("bell", "devices.2.bell", elementPath: "host.0");
}

public sealed class ReturnedBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.DefineReturnedUnsubscriber("bell", "devices.2.bell", "watch", elementPath: "host.0");
}

public sealed class PluginBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.Define("bell", VendorLookup.ByPlugin("ward-bells"), "hear", "forget", elementPath: "host.0");
}

public sealed class PluginCallbackBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.DefineCallbackProperties("bell", VendorLookup.ByPlugin("ward-bells"), elementPath: "host.0");
}

public sealed class PluginReturnedBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.DefineReturnedUnsubscriber("bell", VendorLookup.ByPlugin("ward-bells"), "watch", elementPath: "host.0");
}

public sealed class ElementBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.Define("element-bell", "", "addEventListener", "removeEventListener", elementPath: "");
}

public sealed class Ring : JsObject
{
    public ReadOnly<string> Room => ReadOnly<string>();
    public ReadOnly<string?> Note => ReadOnly<string?>(optional: true);
    public Property<bool> Cancel => Property<bool>();
    public RingDetail Detail => LiveObject<RingDetail>();
    public MethodResult<int> Number() => Method<int>();
}

public sealed class RingDetail : JsObject
{
    public ReadOnly<string> Room => ReadOnly<string>();
    public ReadOnly<string> MisplacedArgument => Argument<string>(0);
    public void MisplacedCall() => MethodAt(1);
    public void MisplacedCallWith(Arg<string> text) => MethodAt(1, [text]);
}

public sealed class Search : JsObject
{
    public ReadOnly<string> Query => Argument<string>(0);
    public ReadOnly<string> Context => Argument<string>(2);
    public ReadOnly<int> BadPosition => Argument<int>(-1);
    public void Show(Arg<string> names, Arg<int> limit) => MethodAt(1, [names, limit]);
    public void Finish() => MethodAt(3);
    public void BadCall() => MethodAt(-1);
    public void BadCallWith(Arg<string> names) => MethodAt(-1, [names]);
}

internal static class BellPlans
{
    internal static string JsonOf(IHtmlContent script)
        => Views.TextOf(script)["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
}

public sealed class OtherPathBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.Define("bell", "devices.3.bell", "hear", "forget", elementPath: "host.0");
}

public sealed class OtherSubscribeBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.Define("bell", "devices.2.bell", "on", "forget", elementPath: "host.0");
}

public sealed class OtherUnsubscribeBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.Define("bell", "devices.2.bell", "hear", "off", elementPath: "host.0");
}

public sealed class OtherElementBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.Define("bell", "devices.2.bell", "hear", "forget", elementPath: "host.1");
}

public sealed class NoElementBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.Define("bell", "devices.2.bell", "hear", "forget");
}

public sealed class EmptyElementBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.Define("bell", "devices.2.bell", "hear", "forget", elementPath: "");
}

public sealed class OtherPluginBell : CallBell
{
    public override ComponentVendor Vendor => ComponentVendor.Define("bell", VendorLookup.ByPlugin("other-bells"), "hear", "forget", elementPath: "host.0");
}
