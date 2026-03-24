namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.HttpMixing
{
    // ── View model ──

    public class HttpMixingModel { }

    // ── Event payload (typed args for When/Then/Else + HTTP pipeline) ──

    public class HttpMixingPayload
    {
        public bool Active { get; set; }
        public int Count { get; set; }
        public string Category { get; set; } = "";
    }

    // ── Response DTOs for typed OnSuccess<T> bindings ──

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
}
