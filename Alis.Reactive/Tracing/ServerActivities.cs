using System.Diagnostics;

namespace Alis.Reactive.Tracing;

internal static class ServerActivities
{
    private static readonly ActivitySource Source = new(ReactiveTracing.SourceName);

    internal static Activity? Render(string planId)
        => Source.StartActivity("alis.render")?.SetTag("alis.plan_id", planId);

    internal static Activity? Check(string validator, string ruleSet)
        => Source.StartActivity("alis.validate")?.SetTag("alis.validator", validator).SetTag("alis.rule_set", ruleSet);

    internal static void Checked(Activity? check, int failures) => check?.SetTag("alis.failure_count", failures);
}
