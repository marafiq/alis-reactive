namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DischargeNoticePageModel
    {
    }

    public class FamilyNoticeRequest
    {
        public string? Channel { get; set; }
    }

    public sealed record DischargeResult(string Message);

    public sealed record FamilyNoticeResult(string Message);
}
