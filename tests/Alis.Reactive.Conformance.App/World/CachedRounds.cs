using System.Globalization;

namespace Alis.Reactive.Conformance.App.World;

internal static class CachedRounds
{
    internal const string Key = "rounds";

    private const string EventTargets = "{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}";

    internal static string AsRenderedAtPlanVersion(int version)
        => "<section aria-label=\"Rounds\">"
           + "<button type=\"button\" id=\"mark-the-round\">Mark the round</button>"
           + "<p id=\"round-status\" role=\"status\"></p>"
           + "<script type=\"application/json\" data-reactive-plan>" + PlanAt(version) + "</script>"
           + "</section>";

    private static string PlanAt(int version)
        => "{\"version\":" + version.ToString(CultureInfo.InvariantCulture) + ",\"planId\":\"Alis.Reactive.Conformance.App.Refusals.Rounds\","
           + "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\","
           + "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}},"
           + "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"},"
           + "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}},\"listen\":" + EventTargets + "}},"
           + "\"objects\":{\"mark-the-round\":{\"kind\":\"element\",\"id\":\"mark-the-round\",\"vendor\":\"native\"},"
           + "\"round-status\":{\"kind\":\"element\",\"id\":\"round-status\",\"vendor\":\"native\"}},"
           + "\"handlers\":[{\"on\":\"mark-the-round\",\"path\":[],\"event\":\"click\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\","
           + "\"do\":[{\"kind\":\"set\",\"path\":[\"textContent\"],\"members\":\"required\",\"content\":true,"
           + "\"target\":{\"kind\":\"obj\",\"id\":\"round-status\"},\"value\":{\"kind\":\"lit\",\"value\":\"done\"}}]}]}";
}
