namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Discharge with a family notice: the coordinator discharges a resident and the page then asks
    /// the server to notify the family.
    /// </summary>
    public class DischargeNoticePageModel
    {
    }

    /// <summary>How the family is reached: "text", "email" or "phone".</summary>
    public class FamilyNoticeRequest
    {
        public string? Channel { get; set; }
    }

    /// <summary>The server's answer to a discharge, or its reason for refusing one.</summary>
    public sealed record DischargeResult(string Message);

    /// <summary>The server's answer to a family notice; the same message envelope as every answer.</summary>
    public sealed record FamilyNoticeResult(string Message);
}
