namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Shift notes: the page lists the residents a nurse visits; each opens in the drawer.
    /// </summary>
    public class ShiftNotePageModel
    {
    }

    /// <summary>The shift note a nurse writes for a resident from the resident's drawer.</summary>
    public class ShiftNoteModel
    {
        public int ResidentId { get; set; }
        public string? Note { get; set; }
    }

    /// <summary>The confirmation the server returns once the shift note is saved.</summary>
    public sealed record ShiftNoteSaved(string Confirmation);

    /// <summary>The confirmation the server returns once the resident is discharged.</summary>
    public sealed record ResidentDischarged(string Confirmation);
}
