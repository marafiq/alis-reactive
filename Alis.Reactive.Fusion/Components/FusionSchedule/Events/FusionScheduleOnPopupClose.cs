namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered after a Syncfusion Schedule popup closes.
    /// </summary>
    public class FusionSchedulePopupCloseArgs
    {
        /// <summary>Popup type that closed, for example <c>QuickInfo</c> or <c>Editor</c>.</summary>
        public string Type { get; set; } = "";

        public FusionSchedulePopupCloseArgs() { }
    }
}
