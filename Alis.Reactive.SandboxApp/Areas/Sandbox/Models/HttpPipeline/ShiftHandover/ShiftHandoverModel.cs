namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Shift handover: the night charge nurse marks each resident's handover note read from a link.
    /// </summary>
    public class ShiftHandoverPageModel
    {
    }

    /// <summary>The server's answer to marking a handover note read.</summary>
    public sealed record HandoverNoteRead(string Message);
}
