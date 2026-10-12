namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.HttpMixing
{
    public class HttpMixingModel
    {
        public string ReactiveResidentName { get; set; } = "";
        public string ReactiveFollowUpName { get; set; } = "";
        public string ReactiveErrorCategory { get; set; } = "";
    }

    public class HttpMixingPayload
    {
        public bool Active { get; set; }
        public int Count { get; set; }
        public string Category { get; set; } = "";
    }

    public class HttpMixingEchoResponse
    {
        public string ReceivedName { get; set; } = "";
        public bool Saved { get; set; }
    }

    public class HttpMixingAuditResponse
    {
        public string Result { get; set; } = "";
    }

    public class HttpMixingClassifyResponse
    {
        public string Tier { get; set; } = "";
    }

    public class HttpMixingResidentAuditResponse
    {
        public int ResidentId { get; set; }
        public string Action { get; set; } = "";
        public string HeaderCategory { get; set; } = "";
    }

    public class HttpMixingAuditTrailResponse
    {
        public int ResidentId { get; set; }
        public string CategorySlug { get; set; } = "";
        public string Step { get; set; } = "";
    }

    /// <summary>The document events the HttpMixing page raises and hears.</summary>
    public static class HttpMixingEvents
    {
        public static readonly AppEvent<HttpMixingPayload> S1SaveThenCheck = AppEvent.Named<HttpMixingPayload>("s1-save-then-check");
        public static readonly AppEvent S12Dispatched = AppEvent.Named("s12-dispatched");
        public static readonly AppEvent<HttpMixingPayload> S12OrderedMixedModules = AppEvent.Named<HttpMixingPayload>("s12-ordered-mixed-modules");
        public static readonly AppEvent S13Dispatched = AppEvent.Named("s13-dispatched");
        public static readonly AppEvent<HttpMixingPayload> S13OrderedParallelMix = AppEvent.Named<HttpMixingPayload>("s13-ordered-parallel-mix");
        public static readonly AppEvent S14Dispatched = AppEvent.Named("s14-dispatched");
        public static readonly AppEvent<HttpMixingPayload> S14RouteTemplateFromEvent = AppEvent.Named<HttpMixingPayload>("s14-route-template-from-event");
        public static readonly AppEvent<HttpMixingPayload> S2ConditionHttpCondition = AppEvent.Named<HttpMixingPayload>("s2-condition-http-condition");
        public static readonly AppEvent<HttpMixingPayload> S3FullInterleave = AppEvent.Named<HttpMixingPayload>("s3-full-interleave");
        public static readonly AppEvent<HttpMixingPayload> S4ConditionInsideSuccess = AppEvent.Named<HttpMixingPayload>("s4-condition-inside-success");
        public static readonly AppEvent<HttpMixingPayload> S5ElseifInsideSuccess = AppEvent.Named<HttpMixingPayload>("s5-elseif-inside-success");
        public static readonly AppEvent<HttpMixingPayload> S6MultiConditionsAfterHttp = AppEvent.Named<HttpMixingPayload>("s6-multi-conditions-after-http");
        public static readonly AppEvent<HttpMixingPayload> S7ConditionInError = AppEvent.Named<HttpMixingPayload>("s7-condition-in-error");
        public static readonly AppEvent<HttpMixingPayload> S8AndGuardInsideSuccess = AppEvent.Named<HttpMixingPayload>("s8-and-guard-inside-success");
    }
}
