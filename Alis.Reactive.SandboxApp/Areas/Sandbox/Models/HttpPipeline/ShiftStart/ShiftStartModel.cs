namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Shift start: the night nurse checks the floor's partners side by side, opens the wing's handover
    /// sheet, orders the night snacks from the kitchen and signs the board.
    /// </summary>
    public class ShiftStartModel
    {
        /// <summary>The wing whose sheet opens and whose board is signed; null until the nurse chooses one.</summary>
        public string? Wing { get; set; }

        /// <summary>The snack the kitchen sends up; null until the nurse chooses one.</summary>
        public string? Snack { get; set; }

        /// <summary>How many the kitchen sends up.</summary>
        public string? SnackCount { get; set; }

        /// <summary>A note for the floor, which stays on the floor.</summary>
        public string? FloorNote { get; set; }
    }

    /// <summary>A partner's answer, in the partner's own words.</summary>
    public sealed record PartnerAnswer(string Answer);

    /// <summary>The query <c>Sheet?wing=</c> binds: whose handover sheet opens.</summary>
    public sealed class SheetRequest
    {
        public string? Wing { get; set; }
    }

    /// <summary>Why no handover sheet opened.</summary>
    public sealed record SheetMessage(string Message);

    /// <summary>A wing's handover sheet: the notes the day shift left.</summary>
    public sealed record HandoverSheet(string Wing, IReadOnlyList<string> Notes);

    /// <summary>The kitchen's answer to an order, or why it refused one.</summary>
    public sealed record KitchenAnswer(string Message);

    /// <summary>The body <c>POST Sign</c> binds: who signs the board, and for which wing.</summary>
    public sealed class BoardSignature
    {
        public string? Nurse { get; set; }

        public string? Wing { get; set; }
    }
}
