namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ShiftStartModel
    {
        public string? Wing { get; set; }

        public string? Snack { get; set; }

        public string? SnackCount { get; set; }

        public string? FloorNote { get; set; }
    }

    public sealed record PartnerAnswer(string Answer);

    public sealed class SheetRequest
    {
        public string? Wing { get; set; }
    }

    public sealed record SheetMessage(string Message);

    public sealed record HandoverSheet(string Wing, IReadOnlyList<string> Notes);

    public sealed record KitchenAnswer(string Message);

    public sealed class BoardSignature
    {
        public string? Nurse { get; set; }

        public string? Wing { get; set; }
    }
}
