namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The evening charge nurse's handover to the night nurse on Maple wing: the handover itself and any
    /// incident the night nurse should read first.
    /// </summary>
    public class NativeTextAreaModel
    {
        public string? HandoverNote { get; set; }

        public string? Incident { get; set; }
    }

    /// <summary>The answer once the handover has reached the night nurse.</summary>
    public sealed class ShiftHandoverSent
    {
        public string Message { get; set; } = "";
    }
}
