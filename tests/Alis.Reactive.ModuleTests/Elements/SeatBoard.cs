using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.ModuleTests.Elements;

public sealed record Seating(string Table);

public sealed class SeatDetail
{
    public string Room { get; init; } = "";
}

public sealed class SeatBoard : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;
    public Event<Seated> Seated => Event<Seated>();
    public MethodResult<string?> Note() => Method<string?>();
}

public sealed class Seated : JsObject
{
    public ReadOnly<string> Type => ReadOnly<string>();
    public ReadOnly<SeatDetail?> Detail => ReadOnly<SeatDetail?>();
    public ReadOnly<string?> Flag => ReadOnly<string?>(optional: true);
    public ReadOnly<decimal> Portion => ReadOnly<decimal>();
}

internal static class SeatPlans
{
    internal const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    internal const string ScriptClose = "</script>";

    private const string Head =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.Elements.Seating","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{""";

    private const string Document =
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\",\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}";

    private const string OnLoad = """},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[""";

    private const string OnSeated = """},"handlers":[{"on":"board","path":[],"event":"seated","payload":[0],"members":"required","policy":"overlap","do":[""";

    private const string Ends = "]}]}";

    internal const string Payload = """{"kind":"ref","name":"e"}""";

    internal static string Element(string id) => "\"" + id + "\":{\"kind\":\"element\",\"id\":\"" + id + "\",\"vendor\":\"native\"}";

    internal static string Obj(string id) => "{\"kind\":\"obj\",\"id\":\"" + id + "\"}";

    internal static string Ref(string name) => "{\"kind\":\"ref\",\"name\":\"" + name + "\"}";

    internal static string Text(string text) => "{\"kind\":\"lit\",\"value\":\"" + text + "\"}";

    internal static string ShownAsText(string expression) => "{\"kind\":\"as\",\"expr\":" + expression + ",\"shape\":{\"kind\":\"string\",\"date\":\"none\",\"negativeZero\":\"0\"}}";

    internal static string WallTime(string text) => "{\"kind\":\"as\",\"expr\":" + Text(text) + ",\"shape\":{\"kind\":\"date\"}}";

    internal static string ShownAsADate(string expression) => "{\"kind\":\"as\",\"expr\":" + expression + ",\"shape\":{\"kind\":\"string\",\"date\":\"date\",\"negativeZero\":\"0\"}}";

    internal static string Loaded(string objects, string statements) => Head + Document + "," + objects + OnLoad + statements + Ends;

    internal static string Seated(string objects, string statements) => Head + objects + OnSeated + statements + Ends;
}
