namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionSchedule.PopupClose (SF "popupClose" event).
    /// Fires when a popup closes.
    /// </summary>
    public class FusionSchedulePopupCloseArgs
    {
        /// <summary>The popup type that closed: "QuickInfo", "Editor", or "DeleteAlert".</summary>
        public string Type { get; set; } = "";

        public FusionSchedulePopupCloseArgs() { }
    }
}
