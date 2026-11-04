namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class PharmacyConnectionModel
    {
        public string? PharmacyId { get; set; }
    }

    public sealed class PharmacyTestOrder
    {
        public string? PharmacyId { get; set; }
    }

    public sealed record PharmacyHandshake(int StatusCode, string ResponseBody);
}
