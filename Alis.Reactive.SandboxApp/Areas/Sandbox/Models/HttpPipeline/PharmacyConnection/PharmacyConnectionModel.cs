namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Pharmacy connection: the administrator sends a test order to a partner pharmacy and reads what the
    /// pharmacy answered.
    /// </summary>
    public class PharmacyConnectionModel
    {
        /// <summary>The partner pharmacy chosen; null until the administrator chooses one.</summary>
        public string? PharmacyId { get; set; }
    }

    /// <summary>
    /// What the pharmacy's system answered the test order, relayed by the server: its HTTP status and
    /// the body of its reply.
    /// </summary>
    public sealed record PharmacyHandshake(int StatusCode, string ResponseBody);
}
