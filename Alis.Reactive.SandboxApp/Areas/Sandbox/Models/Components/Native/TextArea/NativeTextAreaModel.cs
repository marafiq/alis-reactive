namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class NativeTextAreaModel
    {
        public string? HandoverNote { get; set; }

        public string? Incident { get; set; }
    }

    public sealed class ShiftHandoverSent
    {
        public string Message { get; set; } = "";
    }
}
