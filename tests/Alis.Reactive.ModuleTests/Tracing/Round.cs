using System;
using System.Diagnostics;
using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;
using Alis.Reactive.Validation;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.ModuleTests.Tracing;

public sealed record Round(string Ward);

public sealed class DoseOrder
{
    public string Resident { get; set; } = "";

    public int Strength { get; set; }
}

public sealed class RoundValidator : ReactiveValidator<DoseOrder>
{
    public RoundValidator()
    {
        RuleFor(order => order.Strength).LessThanOrEqualTo(500).WithMessage("Strength is above the ward's limit.");
        RuleSet("Night", () => RuleFor(order => order.Resident).NotEmpty().WithMessage("Name the resident."));
    }
}

public sealed class WardBell : Plugin
{
    public WardBell()
        : base("wardbell")
    {
    }

    public Event Rung => Event();
}

public sealed class RoundButton : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Event Pressed => Event(name: "click");
}

internal static class RoundPlans
{
    internal const string TraceId = "0af7651916cd43dd8448eb211c80319c";

    internal const string CallerSpan = "b7ad6b7169203331";

    internal const string Head =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.Tracing.Round","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},""";

    internal const string Body =
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}},\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}},\"objects\":{\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\",\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}},\"round-status\":{\"kind\":\"element\",\"id\":\"round-status\",\"vendor\":\"native\"}},\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true,\"target\":{\"kind\":\"obj\",\"id\":\"round-status\"},\"value\":{\"kind\":\"lit\",\"value\":\"begun\"}}]},{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true,\"target\":{\"kind\":\"obj\",\"id\":\"round-status\"},\"value\":{\"kind\":\"lit\",\"value\":\"done\"}}]}]}";

    internal const string BoardBody =
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}},\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}},\"objects\":{\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\",\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}},\"$plugin:wardbell\":{\"kind\":\"plugin\",\"name\":\"wardbell\",\"key\":\"\",\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}},\"round-done\":{\"kind\":\"element\",\"id\":\"round-done\",\"vendor\":\"native\"},\"round-status\":{\"kind\":\"element\",\"id\":\"round-status\",\"vendor\":\"native\"}},\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true,\"target\":{\"kind\":\"obj\",\"id\":\"round-status\"},\"value\":{\"kind\":\"lit\",\"value\":\"begun\"}}]},{\"on\":\"$plugin:wardbell\",\"path\":[],\"event\":\"rung\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true,\"target\":{\"kind\":\"obj\",\"id\":\"round-status\"},\"value\":{\"kind\":\"lit\",\"value\":\"rung\"}}]},{\"on\":\"round-done\",\"path\":[],\"event\":\"click\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true,\"target\":{\"kind\":\"obj\",\"id\":\"round-status\"},\"value\":{\"kind\":\"lit\",\"value\":\"done\"}}]}]}";

    internal static string JsonOf(IHtmlContent script) => JsonOf(Views.TextOf(script));

    internal static string JsonOf(string rendered)
    {
        const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
        var opens = rendered.IndexOf(ScriptOpen, StringComparison.Ordinal);
        return rendered[(opens + ScriptOpen.Length)..rendered.LastIndexOf("</script>", StringComparison.Ordinal)];
    }

    internal static Activity ARequest(ActivityTraceFlags flags)
    {
        var request = new Activity("GET /Care/Round").SetParentId(ActivityTraceId.CreateFromString(TraceId), ActivitySpanId.CreateFromString(CallerSpan), flags);
        request.TraceStateString = "rojo=00f067aa0ba902b7";
        return request.Start();
    }

    internal static string TraceparentOf(Activity activity) => "00-" + TraceId + "-" + activity.SpanId.ToHexString() + "-01";
}
