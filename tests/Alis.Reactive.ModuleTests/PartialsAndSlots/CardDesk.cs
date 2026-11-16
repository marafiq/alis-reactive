using System;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.ModuleTests.PartialsAndSlots;

public sealed record CardDesk(string Ward);

internal static class CardPlans
{
    internal const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    internal const string ScriptClose = "</script>";

    internal const string Document =
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\",\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}";

    private const string Head =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.PartialsAndSlots.CardDesk","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{""";

    private const string OnLoad = """},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[""";

    private const string Ends = "]}]}";

    private const string None = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}""";

    internal static string Loaded(string objects, string statements) => Head + objects + OnLoad + statements + Ends;

    internal static string Slot(string elementId)
        => "\"$slot:" + elementId + "\":{\"kind\":\"plugin\",\"name\":\"alis.slot\",\"key\":\"" + elementId +
           "\",\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}";

    internal static string Load(string elementId, string html)
        => "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"load\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"$slot:" + elementId + "\"},\"args\":[" + html + "]}}";

    internal static string Unload(string elementId)
        => "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"unload\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"$slot:" + elementId + "\"},\"args\":[]}}";

    internal static string HtmlOfTheReply(string reply)
        => "{\"kind\":\"get\",\"path\":[\"value\"],\"members\":\"optional\",\"target\":{\"kind\":\"get\",\"path\":[\"body\"],\"members\":\"required\",\"target\":{\"kind\":\"ref\",\"name\":\"" + reply + "\"}}}";

    internal static string HtmlRequest(string method, string path, string query, string success)
        => "{\"kind\":\"request\",\"argument\":\"argument1\",\"reply\":\"reply1\",\"send\":{\"kind\":\"record\",\"fields\":[" +
           "{\"name\":\"method\",\"value\":{\"kind\":\"lit\",\"value\":\"" + method + "\"}}," +
           "{\"name\":\"url\",\"value\":{\"kind\":\"record\",\"fields\":[{\"name\":\"parts\",\"value\":{\"kind\":\"list\",\"items\":[{\"kind\":\"record\",\"fields\":[" +
           "{\"name\":\"kind\",\"value\":{\"kind\":\"lit\",\"value\":\"text\"}},{\"name\":\"text\",\"value\":{\"kind\":\"lit\",\"value\":\"" + path + "\"}}]}]}}," +
           "{\"name\":\"query\",\"value\":{\"kind\":\"lit\",\"value\":\"" + query + "\"}}]}}," +
           "{\"name\":\"query\",\"value\":{\"kind\":\"record\",\"fields\":[{\"name\":\"fields\",\"value\":{\"kind\":\"list\",\"items\":[]}},{\"name\":\"inputs\",\"value\":{\"kind\":\"record\",\"fields\":[" +
           "{\"name\":\"container\",\"value\":" + None + "},{\"name\":\"individual\",\"value\":{\"kind\":\"list\",\"items\":[]}}]}}]}}," +
           "{\"name\":\"body\",\"value\":" + None + "}," +
           "{\"name\":\"headers\",\"value\":{\"kind\":\"list\",\"items\":[{\"kind\":\"record\",\"fields\":[{\"name\":\"name\",\"value\":{\"kind\":\"lit\",\"value\":\"Accept\"}}," +
           "{\"name\":\"value\",\"value\":{\"kind\":\"record\",\"fields\":[{\"name\":\"kind\",\"value\":{\"kind\":\"lit\",\"value\":\"held\"}},{\"name\":\"value\",\"value\":{\"kind\":\"lit\",\"value\":\"text/html\"}}]}}," +
           "{\"name\":\"form\",\"value\":{\"kind\":\"record\",\"fields\":[{\"name\":\"kind\",\"value\":{\"kind\":\"lit\",\"value\":\"text\"}}]}},{\"name\":\"when\",\"value\":{\"kind\":\"lit\",\"value\":true}}]}]}}," +
           "{\"name\":\"antiforgery\",\"value\":" + None + "}," +
           "{\"name\":\"retry\",\"value\":" + None + "}," +
           "{\"name\":\"busy\",\"value\":" + None + "}," +
           "{\"name\":\"respondedJson\",\"value\":{\"kind\":\"lit\",\"value\":false}}," +
           "{\"name\":\"contracts\",\"value\":{\"kind\":\"list\",\"items\":[]}}]}," +
           "\"check\":[],\"layers\":[{\"whileLoading\":[],\"finally\":[]}],\"success\":{\"kind\":\"html\",\"do\":[" + success + "]}," +
           "\"statuses\":[],\"otherwise\":{\"kind\":\"none\",\"do\":[]},\"chained\":[]}";

    internal static string Rendered(Action<Pipeline<CardDesk>> write)
    {
        var html = Views.Of(new CardDesk("North"), _ => { }, app => app.AddReactiveRequests(requests => requests.NoAntiforgery()));
        var plan = html.ReactivePlan<CardDesk>();
        html.On(plan, trigger => trigger.DomReady(write));
        return Views.TextOf(html.RenderPlan(plan));
    }
}
