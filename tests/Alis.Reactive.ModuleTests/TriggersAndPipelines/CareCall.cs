using System;
using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.ModuleTests.TriggersAndPipelines;

public sealed record CareCall(string Ward);

public sealed class ResidentFlagged
{
    public int Room { get; set; }

    public string Reason { get; set; } = "unknown";

    public string WardName { get; set; } = "";
}

public enum Severity
{
    Routine,
    Urgent,
}

public sealed class Escalation
{
    public bool Paged { get; set; }

    public int? Bed { get; set; }

    public DateTime Due { get; set; }

    public Severity Level { get; set; }

    public decimal Dose { get; set; }

    public string? Note { get; set; }
}

public static class CareEvents
{
    public static readonly AppEvent RoundStarted = AppEvent.Declare();

    public static readonly AppEvent<ResidentFlagged> Flagged = AppEvent.Declare<ResidentFlagged>();

    public static readonly AppEvent<ResidentFlagged> Acknowledged = AppEvent.Declare<ResidentFlagged>();

    public static readonly AppEvent<Escalation> Escalated = AppEvent.Declare<Escalation>(name: "care-escalated");
}

public sealed class CareDesk : Plugin
{
    public CareDesk()
        : base("caredesk")
    {
    }

    public Property<int> Room => Property<int>();

    public Property<string> Reason => Property<string>();

    public Property<bool> Paged => Property<bool>();

    public Property<int?> Bed => Property<int?>();

    public Property<DateTime> Due => Property<DateTime>();

    public Property<Severity> Level => Property<Severity>();

    public Property<decimal> Dose => Property<decimal>();

    public Property<string?> Note => Property<string?>();
}

internal static class CarePlans
{
    internal const string Head =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.TriggersAndPipelines.CareCall","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{""";

    internal const string Document =
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\",\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}";

    internal const string Desk =
        "\"$plugin:caredesk\":{\"kind\":\"plugin\",\"name\":\"caredesk\",\"key\":\"\",\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}";

    internal const string TheDocument = """{"kind":"obj","id":"$document"}""";

    internal const string TheDesk = """{"kind":"obj","id":"$plugin:caredesk"}""";

    internal const string Payload = """{"kind":"ref","name":"e"}""";

    internal const string AnInteger = """{"kind":"number","of":"integer","text":"invariant","overflow":"nothing"}""";

    internal const string AFraction = """{"kind":"number","of":"fraction","text":"invariant","overflow":"nothing"}""";

    internal static string JsonOf(IHtmlContent script)
        => Views.TextOf(script)["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];

    internal static string Handler(string appEvent, string payload, string policy, string statements)
        => "{\"on\":\"$document\",\"path\":[],\"event\":\"" + appEvent + "\",\"payload\":" + payload + ",\"members\":\"required\",\"policy\":\"" + policy + "\",\"do\":[" + statements + "]}";

    internal static string Plan(string objects, string handlers) => Head + objects + "},\"handlers\":[" + handlers + "]}";

    internal static string SetsTheDesk(string member, string value)
        => "{\"kind\":\"set\",\"path\":[\"" + member + "\"],\"members\":\"required\",\"content\":false,\"target\":" + TheDesk + ",\"value\":" + value + "}";

    internal static string Dispatches(string appEvent, string payload)
        => "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"dispatch\"],\"members\":\"required\",\"target\":" + TheDocument +
            ",\"args\":[{\"kind\":\"lit\",\"value\":\"" + appEvent + "\"}," + payload + "]}}";

    internal static string PayloadMember(string name, string start)
        => "{\"kind\":\"data\",\"path\":[{\"name\":\"" + name + "\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":" + start + "}}],\"target\":" + Payload + "}";

    internal static string ReadAs(string expression, string shape) => "{\"kind\":\"as\",\"expr\":" + expression + ",\"shape\":" + shape + "}";
}
