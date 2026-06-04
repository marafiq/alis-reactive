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
}
