namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class OtpInputModel
    {
        public string? Passcode { get; set; }

        public string? AutoBlurCode { get; set; }
    }

    public sealed class OtpInputEchoRequest
    {
        public string? Passcode { get; set; }
    }

    public sealed class OtpInputEchoResponse
    {
        public string? Passcode { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
