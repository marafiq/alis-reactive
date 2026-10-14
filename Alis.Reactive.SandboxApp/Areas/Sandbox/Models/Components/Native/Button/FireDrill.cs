namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.Button
{
    /// <summary>A wing's monthly fire drill, run by the charge aide from the wing's tablet.</summary>
    public sealed class FireDrill
    {
        /// <summary>The wing the drill runs on.</summary>
        public string Wing { get; set; } = "";
    }

    /// <summary>The document events the fire drill page raises and hears.</summary>
    public static class FireDrillEvents
    {
        /// <summary>A resident is not where the drill expects them.</summary>
        public static readonly AppEvent ResidentMissing = AppEvent.Named("fire-drill-resident-missing");
    }
}
