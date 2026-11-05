namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ShiftNotePageModel
    {
    }

    public class ShiftNoteModel
    {
        public int ResidentId { get; set; }
        public string? Note { get; set; }
    }

    public sealed record ShiftNoteSaved(string Confirmation);

    public sealed record ResidentDischarged(string Confirmation);
}
