using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.ModuleTests.Execution;

public sealed record AlarmDesk(string Ward);

public sealed class AlarmPanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;
    public Event<AlarmRaised> Raised => Event<AlarmRaised>();
    public Event<AlarmPanel> HandedOver => Event<AlarmPanel>();
    public Property<string> Note => Property<string>();
    public void Settle() => MethodAsync();
    public MethodResult<bool> SettleCheck() => MethodAsync<bool>();
    public MethodResult<string> SettleNote() => MethodAsync<string>();
}

public sealed class AlarmRaised : JsObject
{
    public ReadOnly<bool> Flag => ReadOnly<bool>();
    public ReadOnly<int> Count => ReadOnly<int>();
    public ReadOnly<string> Ward => ReadOnly<string>();
    public WriteOnly<string> Note => WriteOnly<string>();
    public AlarmLog Log => LiveObject<AlarmLog>();
    public void Acknowledge() => Method();
}

public sealed class AlarmLog : JsObject
{
    public Property<string> Entry => Property<string>();
}
