namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.Button
{
    public sealed class FireDrill
    {
        public string Wing { get; set; } = "";
    }

    public static class FireDrillEvents
    {
        public static readonly AppEvent ResidentMissing = AppEvent.Declare();
    }
}
