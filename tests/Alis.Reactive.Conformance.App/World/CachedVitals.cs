using System.Globalization;

namespace Alis.Reactive.Conformance.App.World;

internal static class CachedVitals
{
    internal const string Key = "vitals";

    private const int PageVersion = 7;

    private const string EventTargets = "{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}";

    private const string InsideTheButtonsObject = "\"id\":\"record-the-vitals\"";

    internal static string AsRenderedAtPlanVersion(int version) => Fragment(PlanAt(version));

    internal static string CutOffInsideAnObject()
    {
        var whole = PlanAt(PageVersion);
        return Fragment(whole[..whole.IndexOf(InsideTheButtonsObject, StringComparison.Ordinal)]);
    }

    private static string Fragment(string plan)
        => "<section aria-label=\"Vitals\">"
           + "<button type=\"button\" id=\"record-the-vitals\">Record the vitals</button>"
           + "<p id=\"vitals-status\" role=\"status\"></p>"
           + "<script type=\"application/json\" data-reactive-plan>" + plan + "</script>"
           + "</section>";

    private static string PlanAt(int version)
        => "{\"version\":" + version.ToString(CultureInfo.InvariantCulture) + ",\"planId\":\"Alis.Reactive.Conformance.App.Care.Vitals\","
           + "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\","
           + "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}},"
           + "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"},"
           + "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}},\"listen\":" + EventTargets + "}},"
           + "\"objects\":{\"record-the-vitals\":{\"kind\":\"element\",\"id\":\"record-the-vitals\",\"vendor\":\"native\"},"
           + "\"vitals-status\":{\"kind\":\"element\",\"id\":\"vitals-status\",\"vendor\":\"native\"}},"
           + "\"handlers\":[{\"on\":\"record-the-vitals\",\"path\":[],\"event\":\"click\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\","
           + "\"do\":[{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true,"
           + "\"target\":{\"kind\":\"obj\",\"id\":\"vitals-status\"},\"value\":{\"kind\":\"lit\",\"value\":\"recorded\"}}]}]}";
}
