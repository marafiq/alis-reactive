namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ProgressButton
{
    /// <summary>
    /// The evening medication pass on Maple wing, sent to the pharmacy once the nurse has checked it off.
    /// </summary>
    public sealed class FusionProgressButtonModel
    {
        /// <summary>The nurse has checked every resident's medications on this pass.</summary>
        public bool PassChecked { get; set; }

        /// <summary>The pharmacy the pass goes to: Lakeside, Riverside or County.</summary>
        public string Pharmacy { get; set; } = "Lakeside";
    }

    /// <summary>The pass, sent to a pharmacy.</summary>
    public sealed class MedicationPassSendRequest
    {
        public string Pharmacy { get; set; } = string.Empty;
    }

    /// <summary>What the pharmacy answered when the pass reached it.</summary>
    public sealed class MedicationPassSendReply
    {
        /// <summary>The pharmacy's system confirmed it has the whole report, so the upload need not run on.</summary>
        public bool ConfirmedOnReceipt { get; init; }

        public string Reply { get; init; } = string.Empty;
    }
}
