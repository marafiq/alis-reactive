namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Discharge with a family notice: the coordinator discharges a resident and the page then asks
    /// the server to notify the family.
    /// </summary>
    public class DischargeNoticePageModel
    {
    }

    /// <summary>How the family is reached: "text" or "email".</summary>
    public class FamilyNoticeRequest
    {
        public string? Channel { get; set; }
    }

    /// <summary>The server's answer to a discharge.</summary>
    public sealed record DischargeResult(string Message);

    /// <summary>The server's answer to a family notice; the same message envelope as every answer.</summary>
    public sealed record FamilyNoticeResult(string Message);
}
