using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Alis.Reactive.Values;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/CallLog/call-log", "CallLog")]
public sealed class CallLog : Component
{
    public static ComponentVendor LogVendor { get; } = ComponentVendor.DefineCallbackProperties("calllog", objectPath: "log");

    public override ComponentVendor Vendor => LogVendor;

    public void Note(Arg<string> text) => Method([text]);
}

public static class CallLogHtml
{
    public static Rendered<CallLog, TModel> CallLog<TModel>(this ReactivePlan<TModel> plan, string id)
    {
        var log = new TagBuilder("ol");
        log.Attributes["id"] = id;
        log.Attributes["data-call-log"] = "";
        return plan.Render<CallLog>(id, log);
    }
}
