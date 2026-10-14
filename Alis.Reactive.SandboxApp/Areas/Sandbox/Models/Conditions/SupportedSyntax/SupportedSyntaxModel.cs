namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.SupportedSyntax;

public class SupportedSyntaxModel
{
    public decimal RiskScore { get; set; }
}

public class SupportedSyntaxTriggerPayload
{
    public int Score { get; set; }
}

/// <summary>The document events the SupportedSyntax page raises and hears.</summary>
public static class SupportedSyntaxEvents
{
    public static readonly AppEvent SupportedReactiveAudited = AppEvent.Named("supported-reactive-audited");
    public static readonly AppEvent SupportedTriggerAudited = AppEvent.Named("supported-trigger-audited");
    public static readonly AppEvent<SupportedSyntaxTriggerPayload> SupportedTriggerReview = AppEvent.Named<SupportedSyntaxTriggerPayload>("supported-trigger-review");
}
