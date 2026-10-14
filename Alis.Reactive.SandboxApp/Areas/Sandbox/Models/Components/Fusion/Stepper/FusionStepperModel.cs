namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Stepper
{
    /// <summary>
    /// A resident's move-in checklist: paperwork, a room that is ready, then the move-in itself, which
    /// waits for the signed room inspection.
    /// </summary>
    public sealed class FusionStepperModel
    {
        /// <summary>Maintenance and the coordinator have walked the room and signed its inspection.</summary>
        public bool RoomInspectionSigned { get; set; }
    }
}
