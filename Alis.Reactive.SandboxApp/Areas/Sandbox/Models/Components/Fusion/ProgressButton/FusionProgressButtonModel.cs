namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ProgressButton
{
    public sealed class FusionProgressButtonModel
    {
        public bool PassChecked { get; set; }

        public string Pharmacy { get; set; } = "Lakeside";
    }

    public sealed class MedicationPassSendRequest
    {
        public string Pharmacy { get; set; } = string.Empty;
    }

    public sealed class MedicationPassSendReply
    {
        public bool ConfirmedOnReceipt { get; init; }

        public string Reply { get; init; } = string.Empty;
    }
}
